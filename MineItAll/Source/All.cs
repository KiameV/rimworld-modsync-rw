using Harmony;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MineItAll
{
    public class Bootstrapper : ModBase
    {
        private class DesignatorEntry
        {
            private readonly Designator_MineTool designator;

            private readonly KeyBindingDef key;

            public DesignatorEntry(Designator_MineTool designator, KeyBindingDef key)
            {
                this.designator = designator;
                this.key = key;
            }
        }

        private const string ModId = "MineItAll";

        private static ModLogger staticLogger;

        internal static HarmonyInstance HarmonyInstance
        {
            get;
            set;
        }

        private static Bootstrapper Instance
        {
            get;
            set;
        }

        public override string ModIdentifier
        {
            get
            {
                return "MineItAll";
            }
        }

        private static ModLogger Logger
        {
            get
            {
                ModLogger arg_19_0;
                if ((arg_19_0 = Bootstrapper.staticLogger) == null)
                {
                    arg_19_0 = (Bootstrapper.staticLogger = new ModLogger("MineItAll"));
                }
                return arg_19_0;
            }
        }

        protected override bool HarmonyAutoPatch
        {
            get
            {
                return false;
            }
        }

        private Bootstrapper()
        {
            Bootstrapper.Instance = this;
        }

        public override void WorldLoaded()
        {
            this.InjectDesignators();
        }

        private Designator_MineTool InstantiateDesignator(Type designatorType, MinerDesignatorDef designatorDef)
        {
            try
            {
                return (Designator_MineTool)Activator.CreateInstance(designatorType, new object[]
                {
                    designatorDef
                });
            }
            catch (Exception ex)
            {
                Bootstrapper.Logger.ReportException(ex, null, false, string.Format("instantiation of {0} with Def {1}", (designatorType != null) ? designatorType.FullName : "(null)", designatorDef));
            }
            return null;
        }

        private void InjectDesignators()
        {
            int num = 0;
            foreach (MinerDesignatorDef current in DefDatabase<MinerDesignatorDef>.AllDefs)
            {
                if (!current.Injected)
                {
                    List<Designator> allResolvedDesignators = current.Category.AllResolvedDesignators;
                    int num2 = -1;
                    for (int i = 0; i < allResolvedDesignators.Count; i++)
                    {
                        if (allResolvedDesignators[i].GetType() == current.insertAfter)
                        {
                            num2 = i;
                            break;
                        }
                    }
                    if (num2 >= 0)
                    {
                        Designator_MineTool designator_MineTool = this.InstantiateDesignator(current.designatorClass, current);
                        designator_MineTool.icon = current.IconTex;
                        allResolvedDesignators.Insert(num2 + 1, designator_MineTool);
                        num++;
                    }
                    else
                    {
                        Bootstrapper.Logger.Error(string.Format("Failed to inject {0} after {1}", current.defName, current.insertAfter.Name), new object[0]);
                    }
                    current.Injected = true;
                }
            }
            if (num > 0)
            {
                Bootstrapper.Logger.Trace(new object[]
                {
                    "Injected " + num + " designators"
                });
            }
        }
    }

    public class Designator_MineBrush : Designator_MineTool
    {
        private float radius = 1f;

        private Material circleMat
        {
            get;
            set;
        }

        public override int DraggableDimensions
        {
            get
            {
                return 0;
            }
        }

        public Designator_MineBrush(MinerDesignatorDef def) : base(def)
        {
            this.circleMat = def.HighlightTex;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            new Designator_Mine().DesignateMultiCell(this.designateAt(loc));
        }

        private IEnumerable<IntVec3> designateAt(IntVec3 here)
        {
            return GenRadial.RadialCellsAround(here, this.radius, true);
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return AcceptanceReport.WasAccepted;
        }

        public override void DrawMouseAttachments()
        {
            base.DrawMouseAttachments();
            if (Input.GetKeyUp(KeyCode.Q) && this.radius > 0f)
            {
                this.radius -= 0.1f;
            }
            if (Input.GetKeyUp(KeyCode.E) && this.radius < 30f)
            {
                this.radius += 0.1f;
            }
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            this.drawCircle(UI.MouseCell());
        }

        private void drawCircle(IntVec3 pos)
        {
            foreach (IntVec3 current in GenRadial.RadialCellsAround(pos, this.radius, true))
            {
                Graphics.DrawMesh(MeshPool.plane10, current.ToVector3ShiftedWithAltitude(30), Quaternion.identity, this.circleMat, 0);
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (Find.DesignatorManager.SelectedDesignator != null && this.radius == 7.9f)
            {
                this.radius = DefDatabase<ThingDef>.GetNamed("SunLamp", true).specialDisplayRadius;
            }
            else
            {
                this.radius = 7.9f;
            }
            base.ProcessInput(ev);
        }
    }

    public abstract class Designator_MineTool : Designator
    {
        internal readonly MinerDesignatorDef def;

        protected int numThingsDesignated;

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        protected Designator_MineTool(MinerDesignatorDef def)
        {
            this.def = def;
            this.defaultLabel = def.label;
            this.defaultDesc = def.description;
            this.icon = def.IconTex;
            this.useMouseIcon = true;
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.soundSucceeded = def.soundSucceeded;
            this.hotKey = def.hotkeyDef;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!GenGrid.InBounds(c, base.Map))
            {
                return false;
            }
            if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.Mine) != null)
            {
                return AcceptanceReport.WasRejected;
            }
            if (GridsUtility.Fogged(c, base.Map))
            {
                return true;
            }
            Mineable firstMineable = GridsUtility.GetFirstMineable(c, base.Map);
            if (firstMineable == null)
            {
                return Translator.Translate("MessageMustDesignateMineable");
            }
            AcceptanceReport result = this.CanDesignateThing(firstMineable);
            if (result.Accepted)
            {
                return AcceptanceReport.WasAccepted;
            }
            return result;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (!t.def.mineable)
            {
                return false;
            }
            if (base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) != null)
            {
                return AcceptanceReport.WasRejected;
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            base.Map.designationManager.AddDesignation(new Designation(loc, DesignationDefOf.Mine));
        }

        public override void DesignateThing(Thing t)
        {
            this.DesignateSingleCell(t.Position);
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Mining, KnowledgeAmount.SpecificInteraction);
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }

    public class Designator_StripMiner : Designator_MineTool
    {
        private IntVec3 startPoint;

        private int spacing = 4;

        private int spacingY = 4;

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public Designator_StripMiner(MinerDesignatorDef def) : base(def)
        {
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            if (Input.GetMouseButtonDown(0))
            {
                this.startPoint = UI.MouseCell();
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            IntVec3 intVec = c - this.startPoint;
            if (intVec.x % this.spacing == 0 || intVec.z % this.spacingY == 0)
            {
                return base.CanDesignateCell(c);
            }
            return AcceptanceReport.WasRejected;
        }

        public override void DrawMouseAttachments()
        {
            base.DrawMouseAttachments();
            bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!flag && Input.GetKeyDown(KeyCode.Q))
            {
                this.spacingY++;
                return;
            }
            if (!flag && Input.GetKeyDown(KeyCode.E))
            {
                this.spacing++;
                return;
            }
            if (flag && Input.GetKeyDown(KeyCode.E) && this.spacing > 4)
            {
                this.spacing--;
                return;
            }
            if (flag && Input.GetKeyDown(KeyCode.Q) && this.spacingY > 4)
            {
                this.spacingY--;
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            this.spacing = 4;
            this.spacingY = 4;
        }
    }

    public class Designator_VeinMiner : Designator_MineTool
    {
        public override int DraggableDimensions
        {
            get
            {
                return 0;
            }
        }

        public Designator_VeinMiner(MinerDesignatorDef def) : base(def)
        {
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            Map currentMap = Current.Game.CurrentMap;
            if (currentMap == null || !c.InBounds(currentMap))
            {
                return false;
            }
            if (currentMap.designationManager.DesignationAt(c, DesignationDefOf.Mine) == null)
            {
                foreach (Thing current in currentMap.thingGrid.ThingsAt(c))
                {
                    if (!this.isOre(current.def) || c.Fogged(currentMap))
                    {
                        return "Must designate mineable and accessable ore!";
                    }
                }
            }
            return AcceptanceReport.WasAccepted;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            Map currentMap = Current.Game.CurrentMap;
            if (currentMap == null)
            {
                return;
            }
            foreach (Thing current in currentMap.thingGrid.ThingsAt(loc))
            {
                if (this.isOre(current.def))
                {
                    IEnumerable<IntVec3> cells = this.GetVeinCells(loc, current.def);
                    new Designator_Mine().DesignateMultiCell(cells);
                }
            }
        }

        private string IntVec3ToString(IntVec3 v)
        {
            StringBuilder sb = new StringBuilder(v.x.ToString());
            sb.Append(",");
            sb.Append(v.z.ToString());
            return sb.ToString();
        }

        private IEnumerable<IntVec3> GetVeinCells(IntVec3 at, ThingDef oreDef)
        {
            Dictionary<string, IntVec3> dictionary = new Dictionary<string, IntVec3>();
            Stack<IntVec3> stack = new Stack<IntVec3>();
            stack.Push(at);
            while (stack.Count > 0)
            {
                IntVec3 i = stack.Pop();
                foreach (Thing t in base.Map.thingGrid.ThingsAt(i))
                {
                    if (t.def == oreDef)
                    {
                        string key = this.IntVec3ToString(i);
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, i);
                            foreach (IntVec3 j in GenAdjFast.AdjacentCellsCardinal(i))
                            {
                                stack.Push(j);
                            }
                        }
                    }
                }
            }
            return dictionary.Values;
            /*List<IntVec3> list = new List<IntVec3>();
            list.Add(at);
            List<IntVec3> list2 = new List<IntVec3>();
            list2.Add(at);
            List<IntVec3> list3 = new List<IntVec3>();
            IntVec3 item = default(IntVec3);
            while (!GenList.NullOrEmpty<IntVec3>(list2))
            {
                foreach (IntVec3 current in list2)
                {
                    foreach (IntVec3 current2 in GenAdjFast.AdjacentCells8Way(current))
                    {
                        if (!GenList.NullOrEmpty<Thing>(Find.get_VisibleMap().thingGrid.ThingsListAt(current2)))
                        {
                            using (List<Thing>.Enumerator enumerator3 = Find.get_VisibleMap().thingGrid.ThingsListAt(current2).GetEnumerator())
                            {
                                while (enumerator3.MoveNext())
                                {
                                    if (enumerator3.Current.def.Equals(oreType) && !list.Contains(current2))
                                    {
                                        list.Add(current2);
                                        list3.Add(current2);
                                    }
                                }
                            }
                        }
                    }
                    item = current;
                }
                list2.Remove(item);
                list2 = list2.Concat(list3).ToList<IntVec3>();
                list3.Clear();
            }
            return list;*/
        }

        public bool isOre(ThingDef def)
        {
            bool isResource = def != null && def.building != null && def.building.isResourceRock;
            return def != null && def.building != null && def.building.isResourceRock;
        }
    }

    public class MinerDesignatorDef : Def
    {
        public Type designatorClass;

        public string category;

        public Type insertAfter;

        public string iconTex;

        public string highlightTex;

        public SoundDef soundSucceeded;

        public KeyBindingDef hotkeyDef;

        public string messageSuccess;

        public string messageFailure;

        private Texture2D resolvedIconTex;

        private Material resolvedHighlightTex;

        private DesignationCategoryDef resolvedCategory;

        public bool Injected
        {
            get;
            set;
        }

        public Texture2D IconTex
        {
            get
            {
                return this.resolvedIconTex;
            }
        }

        public Material HighlightTex
        {
            get
            {
                return this.resolvedHighlightTex;
            }
        }

        public DesignationCategoryDef Category
        {
            get
            {
                return this.resolvedCategory;
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            this.resolvedCategory = DefDatabase<DesignationCategoryDef>.GetNamed(this.category, true);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.resolvedIconTex = ContentFinder<Texture2D>.Get(this.iconTex, true);
                this.resolvedHighlightTex = MaterialPool.MatFrom(this.highlightTex);
            });
        }

        public override void PostLoad()
        {
            this.Assert(this.designatorClass != null, "designatorClass field must be set");
            this.Assert(this.category != null, "category field must be set");
            this.Assert(this.insertAfter != null, "insertAfter field must be set");
            this.Assert(this.iconTex != null, "icon texture must be set");
        }

        private void Assert(bool check, string errorMessage)
        {
            if (!check)
            {
                Log.Error(string.Format("[AllowTool] Invalid data in MinerDesignatorDef {0}: {1}", this.defName, errorMessage));
            }
        }
    }
}
