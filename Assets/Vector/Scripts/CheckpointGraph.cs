using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace VectorGame
{
    // Keep the existing System.Random sequence; one internal draw per used API call.
    public sealed class CheckpointRandom : System.Random
    {
        public readonly int Seed;
        public int Draws {get;private set;}
        public CheckpointRandom(int seed,int draws=0):base(seed)
        {if(draws<0||draws>2000000)throw new ArgumentException("Invalid random state");Seed=seed;for(int i=0;i<draws;i++)base.Next();Draws=draws;}
        public override int Next(){Draws++;return base.Next();}
        public override int Next(int max){Draws++;return base.Next(max);}
        public override int Next(int min,int max){if((long)max-min>int.MaxValue)throw new ArgumentOutOfRangeException();Draws++;return base.Next(min,max);}
        public override double NextDouble(){Draws++;return base.NextDouble();}
        public override void NextBytes(byte[] buffer){Draws+=buffer.Length;base.NextBytes(buffer);}
    }
    [Serializable] public sealed class SavedMember {public string name;public int value;}
    [Serializable] public sealed class SavedObject {public string type,text;public List<SavedMember> fields=new List<SavedMember>();public List<int> values=new List<int>();}
    [Serializable] public sealed class SavedGraph {public List<SavedMember> roots=new List<SavedMember>();public List<SavedObject> objects=new List<SavedObject>();}

    // Only the transitive field types of the explicit checkpoint roots are accepted.
    // No save-supplied CLR type name is resolved or instantiated outside this registry.
    public sealed class CheckpointGraph
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly;
        readonly Dictionary<string,Type> types=new Dictionary<string,Type>();
        readonly Dictionary<object,int> written=new Dictionary<object,int>(new Identity());
        readonly Dictionary<string,FieldInfo> roots=new Dictionary<string,FieldInfo>();
        readonly Func<string,ItemDefinition> definition;
        SavedGraph graph;
        object[] values;
        sealed class Identity:IEqualityComparer<object>
        {public new bool Equals(object a,object b)=>ReferenceEquals(a,b);public int GetHashCode(object a)=>System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(a);}
        static bool Skip(FieldInfo f)=>f.IsStatic||typeof(Delegate).IsAssignableFrom(f.FieldType)||typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)||f.FieldType==typeof(FighterRig)||f.FieldType==typeof(FighterPhysics);
        static FieldInfo[] Fields(Type t)=>t.GetFields(Flags).Where(f=>!Skip(f)).OrderBy(f=>f.Name,StringComparer.Ordinal).ToArray();
        static bool Atomic(Type t)=>t.IsPrimitive||t.IsEnum||t==typeof(string)||t==typeof(Vector3)||t==typeof(Quaternion)||t==typeof(Color);
        static bool Container(Type t)=>t.IsArray||t.IsGenericType&&(t.GetGenericTypeDefinition()==typeof(List<>)||t.GetGenericTypeDefinition()==typeof(Dictionary<,>)||t.GetGenericTypeDefinition()==typeof(HashSet<>)||t.GetGenericTypeDefinition()==typeof(Queue<>));
        public CheckpointGraph(Type root,string names,Func<string,ItemDefinition> definition)
        {
            this.definition=definition;
            foreach(string name in names.Split(new[]{' ','\n','\r'},StringSplitOptions.RemoveEmptyEntries))
            {
                var field=root.GetField(name,Flags)??root.GetField("<"+name+">k__BackingField",Flags);
                if(field==null)throw new InvalidOperationException("Missing checkpoint field: "+name);
                roots.Add(name,field);Register(field.FieldType);
            }
            Register(typeof(CheckpointRandom));
        }
        void Register(Type t)
        {
            if(types.ContainsKey(t.FullName))return;
            if(!(Atomic(t)||Container(t)||t==typeof(System.Random)||t==typeof(CheckpointRandom)||t.Namespace=="VectorGame"))throw new InvalidOperationException("Unsupported checkpoint type: "+t);
            types.Add(t.FullName,t);
            if(Atomic(t)||t==typeof(ItemDefinition)||t==typeof(System.Random)||t==typeof(CheckpointRandom))return;
            if(t.IsArray){Register(t.GetElementType());return;}
            if(Container(t)){foreach(var a in t.GetGenericArguments())Register(a);return;}
            foreach(var f in Fields(t))Register(f.FieldType);
        }
        public SavedGraph Capture(object root)
        {
            graph=new SavedGraph();written.Clear();
            foreach(var pair in roots)graph.roots.Add(new SavedMember{name=pair.Key,value=Write(pair.Value.GetValue(root))});
            return graph;
        }
        int Write(object value)
        {
            if(value==null)return -1;if(written.TryGetValue(value,out int known))return known;
            var t=value.GetType();if(!types.ContainsKey(t.FullName))throw new InvalidOperationException("Unregistered checkpoint value: "+t);
            int id=graph.objects.Count;var node=new SavedObject{type=t.FullName};graph.objects.Add(node);written.Add(value,id);
            if(value is ItemDefinition def)node.text=def.id;
            else if(value is CheckpointRandom random)node.text=random.Seed+","+random.Draws;
            else if(t==typeof(Vector3)||t==typeof(Quaternion)||t==typeof(Color))node.text=JsonUtility.ToJson(value);
            else if(t.IsEnum)node.text=Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
            else if(value is float single)node.text=single.ToString("R",CultureInfo.InvariantCulture);
            else if(value is double number)node.text=number.ToString("R",CultureInfo.InvariantCulture);
            else if(Atomic(t))node.text=Convert.ToString(value,CultureInfo.InvariantCulture);
            else if(value is IDictionary dictionary){foreach(DictionaryEntry entry in dictionary){node.values.Add(Write(entry.Key));node.values.Add(Write(entry.Value));}}
            else if(Container(t)){foreach(var entry in (IEnumerable)value)node.values.Add(Write(entry));}
            else foreach(var f in Fields(t))node.fields.Add(new SavedMember{name=f.Name,value=Write(f.GetValue(value))});
            return id;
        }
        object Ref(int id){if(id== -1)return null;if(id<0||id>=values.Length)throw new InvalidOperationException("Invalid checkpoint reference");return values[id];}
        public void Decode(SavedGraph data,bool legacyCrews=false)
        {
            if(data==null||data.objects==null||data.objects.Count>120000||data.roots.Count!=roots.Count||data.roots.Select(x=>x.name).Distinct().Count()!=roots.Count)throw new InvalidOperationException("Invalid checkpoint graph");
            graph=data;values=new object[data.objects.Count];
            foreach(var root in data.roots)if(!roots.ContainsKey(root.name))throw new InvalidOperationException("Unsupported checkpoint field");
            for(int i=0;i<values.Length;i++)
            {
                var n=data.objects[i];if(!types.TryGetValue(n.type,out var t))throw new InvalidOperationException("Unsupported checkpoint type");
                if(n.values.Count>120000||n.fields.Count>300)throw new InvalidOperationException("Checkpoint collection too large");
                if(t==typeof(ItemDefinition))values[i]=definition(n.text)??throw new InvalidOperationException("Unknown saved item");
                else if(t==typeof(CheckpointRandom)){var parts=n.text.Split(',');values[i]=new CheckpointRandom(int.Parse(parts[0]),int.Parse(parts[1]));}
                else if(t==typeof(System.Random))throw new InvalidOperationException("Untracked random source");
                else if(t==typeof(Vector3)||t==typeof(Quaternion)||t==typeof(Color))values[i]=JsonUtility.FromJson(n.text,t);
                else if(t.IsEnum)values[i]=Enum.ToObject(t,long.Parse(n.text,CultureInfo.InvariantCulture));
                else if(Atomic(t))values[i]=Convert.ChangeType(n.text,t,CultureInfo.InvariantCulture);
                else if(t.IsArray)values[i]=Array.CreateInstance(t.GetElementType(),n.values.Count);
                else if(Container(t))values[i]=Activator.CreateInstance(t);
                else values[i]=FormatterServices.GetUninitializedObject(t);
            }
            for(int i=0;i<values.Length;i++)
            {
                var n=data.objects[i];var t=types[n.type];if(Atomic(t)||t==typeof(ItemDefinition)||t==typeof(CheckpointRandom)||Container(t))continue;
                var fields=Fields(t);
                bool Optional(FieldInfo f)=>legacyCrews&&t.FullName=="VectorGame.VectorEncounter+Enemy"&&new[]{"crew","crewClock","crewCooldown","crewVfx","crewNotice","crewGuard","crewGuardUntil","freightLeft","crewTarget","crewUses","freightSource","crewAim","crewOrigin","freightPrevious","freightHitHero","freightHits"}.Contains(f.Name);
                if(n.fields.Any(x=>!fields.Any(f=>f.Name==x.name))||n.fields.Select(x=>x.name).Distinct().Count()!=n.fields.Count)throw new InvalidOperationException("Checkpoint version differs");
                foreach(var f in fields){var saved=n.fields.SingleOrDefault(x=>x.name==f.Name);if(saved==null){if(Optional(f))continue;throw new InvalidOperationException("Checkpoint version differs");}f.SetValue(values[i],Ref(saved.value));}
            }
            for(int i=0;i<values.Length;i++)
            {
                var n=data.objects[i];var t=types[n.type];if(!Container(t))continue;
                if(values[i] is IDictionary dictionary){if(n.values.Count%2!=0)throw new InvalidOperationException("Invalid dictionary");for(int j=0;j<n.values.Count;j+=2)dictionary.Add(Ref(n.values[j]),Ref(n.values[j+1]));}
                else if(values[i] is Array array){for(int j=0;j<n.values.Count;j++)array.SetValue(Ref(n.values[j]),j);}
                else {var add=t.GetMethod(t.GetGenericTypeDefinition()==typeof(Queue<>)?"Enqueue":"Add");foreach(int id in n.values)add.Invoke(values[i],new[]{Ref(id)});}
            }
        }
        public object Root(string name)=>Ref(graph.roots.Single(x=>x.name==name).value);
        public void Apply(object root){foreach(var entry in graph.roots)roots[entry.name].SetValue(root,Ref(entry.value));}
    }
}
