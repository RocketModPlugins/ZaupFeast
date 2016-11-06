using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Effects;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
namespace ZaupFeast
{
    internal class Feast : RocketPlugin<FeastConfiguration>
    {
        public static Feast Instance = null;
        private DateTime nextFeast;
        private List<Locs> locations = new List<Locs>();
        internal Locs nextLocation;
        private byte msgNum;
        private DateTime lastMsg;
        private bool running = false;
        private System.Timers.Timer Timer;
        private System.Timers.Timer Timer2;
        private byte effectNum;
        private List<Vector3> drops;
        private List<Item> items;
        private int numItems;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"coming_feast_msg","The feast is beginning at {0} in {1} minutes!"},
                    {"now_feast_msg","The feast is now at {0}!"}
                };
            }
        }
        
        protected override void Load()
        {
            // Make this able to be used a singleton
            Feast.Instance = this;

            // Set some variables to empty to start.
            this.drops = new List<Vector3>();
            this.items = new List<Item>();
            this.numItems = 0;
            this.Timer = new System.Timers.Timer(11500);
            this.Timer.Elapsed += this.Timer_Elapsed;
            this.Timer.AutoReset = false;
            this.Timer2 = new System.Timers.Timer(12800);
            this.Timer2.Elapsed += this.Timer_Elapsed;
            this.Timer2.AutoReset = false;
            this.effectNum = 0;

            // Did we load the configuration file?  If not, just end.
            if (Configuration.Instance.Items == null || ! Configuration.Instance.Items.Any())
            {
                Rocket.Core.Logging.Logger.Log("Failed to load the configuration file.  Turned off feast.  Restart to try again.");
                return;
            }

           // if (LevelNodes.nodes == null)
            //    LevelNodes.load();
        }

        protected override void Unload()
        {
            Feast.Instance.Timer.Close();
            Feast.Instance.Timer2.Close();
        }

        internal void runFeast()
        {
            // Get how many items are going to drop and setup the item id list.
            int itemsNum = UnityEngine.Random.Range((int)Feast.Instance.Configuration.Instance.MinItemsDrop, (int)Feast.Instance.Configuration.Instance.MaxItemsDrop+1);
            List<int> list = new List<int>();
            foreach (FeastItem current in Feast.Instance.Configuration.Instance.Items)
            {
                if (current.Location.Contains(Feast.Instance.nextLocation.Name) || current.Location.Contains("all") || current.Location.Contains("All")) 
                {
                    for (byte b = 0; b < current.Chance; b++)
                    {
                        list.Add(current.Id);
                    }
                }
            }

            // Figure out the base point of the feast drop.
            Vector3 p = Feast.Instance.nextLocation.Pos;
            /*p.y = 500;
            RaycastHit hit;
            Physics.Raycast(p, Vector3.down, out hit, 500);
            p = hit.point;*/

            // Get the ending item positions and items
            List<Vector3> drops = new List<Vector3>();
            List<Item> items = new List<Item>();
            for (int i = 0; i < itemsNum; i++)
            {
                int item = UnityEngine.Random.Range(0, list.Count);
                int num2 = list[item];
                Vector3 pos1 = new Vector3();
                pos1.x += p.x + (float)UnityEngine.Random.Range((int)Feast.Instance.Configuration.Instance.DropRadius * -1, (int)Feast.Instance.Configuration.Instance.DropRadius + 1);
                pos1.z += p.z + (float)UnityEngine.Random.Range((int)Feast.Instance.Configuration.Instance.DropRadius * -1, (int)Feast.Instance.Configuration.Instance.DropRadius + 1);
                pos1.y = 200;
                RaycastHit hit;
                Physics.Raycast(pos1, Vector3.down, out hit, 500);
                Vector3 pos = hit.point;
                Item g = new Item((ushort)num2, true);
                //drops[i] = pos;
                drops.Add(pos);
                //items[i] = g;
                items.Add(g);
                list.RemoveAt(item);
            }
            Feast.Instance.numItems = itemsNum;
            Feast.Instance.drops = drops;
            Feast.Instance.items = items;
            // See if skydrop is turned on.  If it is, let's start the effects.
            
            if (Feast.Instance.Configuration.Instance.SkyDrop)
            {
                EffectAsset plane = Assets.find(EAssetType.EFFECT, Feast.Instance.Configuration.Instance.PlaneEffectId) as EffectAsset;

                if (plane == null)
                {
                    Logger.Log("The skydrop plane bundle is not the server!  Cannot trigger the airdrop animation.  Just sending items.");
                    this.spawnItems(this.items, drops);
                }
                else
                {
                    Vector3 loc = new Vector3(Feast.Instance.nextLocation.Pos.x, Feast.Instance.nextLocation.Pos.y + 50, Feast.Instance.nextLocation.Pos.z + 500);
                    SDG.Unturned.EffectManager.instance.channel.send("tellEffectPoint", ESteamCall.CLIENTS, loc, 1024, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, new object[] { Feast.Instance.Configuration.Instance.SkydropEffectId, loc });
                    Feast.Instance.effectNum = 1;
                    Feast.Instance.Timer.Start();
                }
            }
            else
            {
                // No effects, just spawn in the items on the ground.
                this.spawnItems(this.items, drops);
            }
            
            // Reset the feast for the next round.
            Feast.Instance.resetFeast();
        }

        private void skyDrop(List<Vector3> pos)
        {
            foreach (Vector3 p in pos)
            {
                Vector3 position = new Vector3(p.x, p.y + 45, p.z);
                SDG.Unturned.EffectManager.instance.channel.send("tellEffectPoint", ESteamCall.CLIENTS, position, 1024, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, new object[] { Feast.Instance.Configuration.Instance.SkydropEffectId, position });
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Feast.Instance.effectNum == 1)
            {
                Feast.Instance.Timer.Stop();
                skyDrop(Feast.Instance.drops);
                Feast.Instance.effectNum = 2;
                Feast.Instance.Timer2.Start();
            }
            else
            {
                Feast.Instance.Timer2.Stop();
                Feast.Instance.spawnItems(Feast.Instance.items, Feast.Instance.drops);
                Feast.Instance.effectNum = 1;
            }
        }

        private void spawnItems(List<Item> items, List<Vector3> drops)
        {
            if (items.Count != drops.Count)
            {
                Logger.Log("Something went wrong.  Feast didn't drop!");
                return;
            }
            for (int d = 0; d < items.Count; d++ ) {
                ItemManager.dropItem(items[d], drops[d], false, true, true);
            }
        }

        private void checkFeast()
        {
            try
            {
                if (Feast.Instance == null) Logger.LogError("Feast.Instance is NULL");
                if (Feast.Instance.Configuration == null) Logger.LogError("(Feast.Instance.Configuration is NULL");
                if (Feast.Instance.Configuration.Instance == null) Logger.LogError("Feast.Instance.Configuration.Instance is NULL");
                if (Feast.Instance.Configuration.Instance.Enabled)
                {
                    if (Feast.Instance.nextFeast == null) Logger.LogError("Feast.Instance.nextFeast is NULL");
                    if (Feast.Instance.lastMsg == null) Logger.LogError("Feast.Instance.lastMsg is NULL");
                    if ((Feast.Instance.nextFeast - DateTime.Now).TotalSeconds - 300.0 <= 0.0 && (DateTime.Now - Feast.Instance.lastMsg).TotalSeconds >= 60.0)
                    {
                        byte b = Feast.Instance.msgNum;
                        if (b != 0)
                        {
                            UnturnedChat.Say(Feast.Instance.Translate("coming_feast_msg",Feast.Instance.nextLocation.Name, Feast.Instance.msgNum), UnturnedChat.GetColorFromName(Feast.Instance.Configuration.Instance.MessageColor, Color.red));
                            Feast.Instance.lastMsg = DateTime.Now;
                            Feast.Instance.msgNum -= 1;
                        }
                        else
                        {
                            UnturnedChat.Say(Feast.Instance.Translate("now_feast_msg", Feast.Instance.nextLocation.Name), UnturnedChat.GetColorFromName(Feast.Instance.Configuration.Instance.MessageColor, Color.red));
                            Feast.Instance.lastMsg = DateTime.Now;
                            Feast.Instance.runFeast();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception in checkFeast");
            }
        }
        public void resetFeast()
        {
            Feast.Instance.nextFeast = DateTime.Now.AddSeconds((double)UnityEngine.Random.Range(Feast.Instance.Configuration.Instance.MinDropTime, Feast.Instance.Configuration.Instance.MaxDropTime + 1));
            Feast.Instance.msgNum = 5;
            Feast.Instance.lastMsg = DateTime.Now;
            Feast.Instance.nextLocation = Feast.Instance.locations[UnityEngine.Random.Range(0, Feast.Instance.locations.Count)];
            Logger.Log("The next feast will be at " + Feast.Instance.nextLocation.Name + " at " + Feast.Instance.nextFeast);
        }

        private void initializeNodes()
        {
            nodesInitialised = true;
            if (LevelNodes.nodes == null) Logger.LogError("Doh! LevelNodes.nodes is NULL");
            foreach (Node n in LevelNodes.nodes)
            {
                if (n.type == ENodeType.LOCATION)
                {
                    Locs loc = new Locs(n.point, ((LocationNode)n).name);
                    this.locations.Add(loc);
                }
            }

            // Get all the locations used by the items and remove any invalid items.
            List<string> usedlocs = new List<string>();

            if (Configuration == null) Logger.LogError("Doh! Configuration is NULL");
            if (Configuration.Instance == null) Logger.LogError("Doh! Configuration.Instance is NULL");
            if (Configuration.Instance.Items == null) Logger.LogError("Configuration.Instance.Items is NULL");
            foreach (FeastItem f in Configuration.Instance.Items.ToList())
            {
                ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, f.Id);
                if (itemAsset == null && itemAsset.isPro)
                {
                    Configuration.Instance.Items.Remove(f);
                }
                else
                {
                    usedlocs = usedlocs.Concat(f.Location).ToList();
                }
            }


            // Remove any unused map locations from the location list.
            List<string> locations = usedlocs.Distinct().ToList();
            List<Locs> locs2 = this.locations.ToList();
            if (!locations.Contains("all") && !locations.Contains("All"))
            {
                foreach (Locs a in locs2)
                {
                    if (locations.IndexOf(a.Name) == -1)
                    {
                        this.locations.Remove(a);
                    }
                }
            }

            // Now that it is all set up, we'll all the plugin to start running.  This was just to keep the FixedUpdate from running before the top finished running.
            this.running = true;
            this.resetFeast();
        }

        

        private bool nodesInitialised = false;
        private DateTime startTime = DateTime.Now;

        public void FixedUpdate()
        {
            try
            {
                if(State == PluginState.Loaded)
                {
                    if (nodesInitialised)
                    {
                        if (Feast.Instance.Configuration.Instance.Enabled && Feast.Instance.running)
                            Feast.Instance.checkFeast();
                    }
                    else
                    {
                        if ((DateTime.Now - startTime).TotalSeconds > 5)
                            initializeNodes();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception in FixedUpdate");
            }
        }
    }

    internal class Locs
    {
        private Vector3 v;
        private string n;
        public Vector3 Pos
        {
            get
            {
                return this.v;
            }
        }
        public string Name
        {
            get
            {
                return this.n;
            }
        }
        public Locs(Vector3 Pos, string Name)
        {
            this.v = Pos;
            this.n = Name;
        }
    }

    public class FeastItem
    {
        public ushort Id;
        public string Name;
        public byte Chance;
        [XmlArray(ElementName = "Locations")]
        public List<string> Location;
        public FeastItem(ushort id, string name, byte chance, List<string> locs)
        {
            this.Id = id;
            this.Name = name;
            this.Chance = chance;
            this.Location = locs;
        }
        public FeastItem()
        {
        }
    }
}
