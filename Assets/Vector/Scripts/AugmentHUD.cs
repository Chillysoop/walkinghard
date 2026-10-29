using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        Rect SocketRect(Rect rect)=>new Rect(rect.xMax-23,rect.yMax-20,18,16);
        LoadoutItem Host(DropRegion r)=>r.socket?game.Deck.ModuleHost(r.item):r.item?.def.kind==ItemKind.Basic?game.Deck.WeaponFor(r.item):r.place==PlaceKind.Weapon?game.Deck.WeaponFor(game.Deck.Basics[r.index]):r.item;
        bool SharedSocket(DropRegion r)=>((r.place==PlaceKind.Weapon&&r.index==1)||(r.item?.def.kind==ItemKind.Basic&&r.item.mount==1))&&game.Deck.WeaponFor(game.Deck.Basics[0])==game.Deck.WeaponFor(game.Deck.Basics[1]);
        void AddSocketRegions()
        {
            foreach(var r in dropRegions.ToArray())
            {
                if(r.offer>=0||r.socket)continue;var host=Host(r);if(host?.Augmentable!=true)continue;
                if(SharedSocket(r))continue;
                if(host.augment!=null)dropRegions.Insert(0,new DropRegion{rect=SocketRect(r.rect),item=host.augment,place=PlaceKind.Inventory,socket=true});
            }
        }
        void DrawSockets()
        {
            foreach(var r in dropRegions)
            {
                if(r.offer>=0||r.socket)continue;var host=Host(r);if(host?.Augmentable!=true)continue;
                bool shared=SharedSocket(r);
                Rect badge=SocketRect(r.rect);Border(badge,host.augment==null?muted:Violet,1);Label(badge,shared?"=":host.augment==null?"+":"◆",11,host.augment==null?muted:Violet,true,TextAnchor.MiddleCenter);
                if(host.augment!=null&&!shared)ItemTooltip(badge,host.augment);
            }
            ItemDefinition pending=dragItem?.def.kind==ItemKind.Augment?dragItem.def:selectedItem?.def.kind==ItemKind.Augment?selectedItem.def:game.Deck.Selected>=0&&!game.Deck.RewardClaimed&&game.Deck.Offers[game.Deck.Selected].def.kind==ItemKind.Augment?game.Deck.Offers[game.Deck.Selected].def:null;
            if(pending!=null)foreach(var r in dropRegions)if(r.offer<0&&!r.socket&&Host(r) is LoadoutItem h&&pending.module.Fits(h.def))Outline(r.rect,Violet,6,2);
        }
        bool MoveToRegion(LoadoutItem item,DropRegion region)
        {
            if(item.def.kind==ItemKind.Augment&&Host(region) is LoadoutItem host&&host.Augmentable)return game.Deck.Attach(item,host);
            return game.MoveItem(item,region.place,region.index);
        }
        float DisplayShare(LoadoutItem item)
        {
            var d=game.Deck;if(item.def.kind!=ItemKind.Weapon||d.TwoHanded)return 1;
            for(int i=0;i<5;i++)if(d.Slots[i]?.def.kind==ItemKind.Basic&&d.WeaponFor(d.Slots[i])==item){int pair=d.PairAfter(i);if(pair>=0&&d.WeaponFor(d.Slots[pair])!=item)return .5f;int before=(i+4)%5;if(d.PairAfter(before)==i&&d.WeaponFor(d.Slots[before])!=item)return .5f;}return 1;
        }
        string ModuleDetails(LoadoutItem module)
        {
            var hosts=game.Deck.CompatibleHosts(module.def).ToArray();string text=module.def.description+"\nCost: "+module.def.tradeoff+"\n\nCOMPATIBLE OWNED ITEMS";
            foreach(var host in hosts){var preview=new LoadoutItem(host.def,host.rank){augment=module};text+="\n"+host.Name+"\n"+AugmentCatalog.Values(preview,DisplayShare(host)).Split('\n')[0];}
            text+="\n\nNo independent rarity or fusion. Scales once from host. Drag onto a highlighted host. Drag ◆ to storage to remove.";return text;
        }
    }
}
