namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicLayoutComponent : LogicComponent
    {
        private readonly LogicVector2[] m_layoutPosition;
        private readonly LogicVector2[] m_editModeLayoutPosition;

        public LogicLayoutComponent(LogicGameObject gameObject) : base(gameObject)
        {
            this.m_layoutPosition = new LogicVector2[8];
            this.m_editModeLayoutPosition = new LogicVector2[8];

            for (int i = 0; i < 8; i++)
            {
                this.m_layoutPosition[i] = new LogicVector2(-1, -1);
                this.m_editModeLayoutPosition[i] = new LogicVector2(-1, -1);
            }
        }

        public override void Destruct()
        {
            base.Destruct();

            for (int i = 0; i < this.m_layoutPosition.Length; i++)
            {
                if (this.m_layoutPosition[i] != null)
                {
                    this.m_layoutPosition[i].Destruct();
                    this.m_layoutPosition[i] = null;
                }
            }

            for (int i = 0; i < this.m_editModeLayoutPosition.Length; i++)
            {
                if (this.m_editModeLayoutPosition[i] != null)
                {
                    this.m_editModeLayoutPosition[i].Destruct();
                    this.m_editModeLayoutPosition[i] = null;
                }
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.LAYOUT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicLevel level = this.m_parent.GetLevel();

            int villageType = this.m_parent.GetVillageType();
            int activeLayout = level.GetActiveLayout();

            for (int i = 0; i < 8; i++)
            {
                if (i == activeLayout)
                {
                    this.m_layoutPosition[i].Set(this.m_parent.GetTileX(), this.m_parent.GetTileY());
                }
                else
                {
                    LogicJSONNumber xNumber = jsonObject.GetJSONNumber(this.GetLayoutVariableNameX(i, false));
                    LogicJSONNumber yNumber = jsonObject.GetJSONNumber(this.GetLayoutVariableNameY(i, false));

                    if (xNumber != null && yNumber != null)
                    {
                        this.m_layoutPosition[i].Set(xNumber.GetIntValue(), yNumber.GetIntValue());
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (level.GetLayoutState(i, villageType) == 1)
                {
                    LogicJSONNumber xNumber = jsonObject.GetJSONNumber(this.GetLayoutVariableNameX(i, true));
                    LogicJSONNumber yNumber = jsonObject.GetJSONNumber(this.GetLayoutVariableNameY(i, true));

                    if (xNumber != null && yNumber != null)
                    {
                        this.m_editModeLayoutPosition[i].Set(xNumber.GetIntValue(), yNumber.GetIntValue());
                    }
                }
            }
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            LogicLevel level = this.m_parent.GetLevel();
            LogicGameMode gameMode = level.GetGameMode();

            if (gameMode.GetVisitType() == 1 ||
                gameMode.GetVisitType() == 4 ||
                gameMode.GetVisitType() == 5)
            {
                int idx = 7;

                if (gameMode.GetVisitType() != 4 || !level.IsArrangedWar())
                {
                    int warLayout = level.GetWarLayout();

                    if (warLayout < 0 || !level.IsWarBase())
                    {
                        idx = level.GetActiveLayout();
                    }
                    else
                    {
                        idx = warLayout;
                    }
                }

                LogicJSONNumber xNumber = jsonObject.GetJSONNumber(this.GetLayoutVariableNameX(idx, false));
                LogicJSONNumber yNumber = jsonObject.GetJSONNumber(this.GetLayoutVariableNameY(idx, false));

                if (xNumber != null && yNumber != null)
                {
                    this.m_parent.SetInitialPosition(xNumber.GetIntValue() << 9, yNumber.GetIntValue() << 9);
                    Debugger.DoAssert(idx < 8, "Layout index out of bands");
                    this.m_layoutPosition[idx].Set(xNumber.GetIntValue(), yNumber.GetIntValue());
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            LogicLevel level = this.m_parent.GetLevel();

            int activeLayout = this.m_parent.GetLevel().GetActiveLayout(villageType);

            for (int i = 0; i < 8; i++)
            {
                LogicVector2 pos = this.m_editModeLayoutPosition[i];

                if (pos.m_x != -1 && pos.m_y != -1)
                {
                    if (level.GetLayoutState(i, villageType) == 1)
                    {
                        jsonObject.Put(this.GetLayoutVariableNameX(i, true), new LogicJSONNumber(pos.m_x));
                        jsonObject.Put(this.GetLayoutVariableNameY(i, true), new LogicJSONNumber(pos.m_y));
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (i != activeLayout)
                {
                    LogicVector2 pos = this.m_layoutPosition[i];

                    if (pos.m_x != -1 && pos.m_y != -1)
                    {
                        jsonObject.Put(this.GetLayoutVariableNameX(i, false), new LogicJSONNumber(pos.m_x));
                        jsonObject.Put(this.GetLayoutVariableNameY(i, false), new LogicJSONNumber(pos.m_y));
                    }
                }
            }
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            if (this.m_parent.GetLevel().IsWarBase() || layoutId > 5 || layoutId == 0 || layoutId == 2 || layoutId == 3)
            {
                Debugger.DoAssert(layoutId < 8, "Layout index out of bounds");

                jsonObject.Put("x", new LogicJSONNumber(this.m_layoutPosition[layoutId].m_x));
                jsonObject.Put("y", new LogicJSONNumber(this.m_layoutPosition[layoutId].m_y));
            }
            else
            {
                jsonObject.Put("x", new LogicJSONNumber(this.m_parent.GetTileX()));
                jsonObject.Put("y", new LogicJSONNumber(this.m_parent.GetTileY()));
            }
        }

        public LogicVector2 GetPositionLayout(int idx)
        {
            Debugger.DoAssert(idx < 8, "Layout index out of bounds");
            return this.m_layoutPosition[idx];
        }

        public LogicVector2 GetEditModePositionLayout(int idx)
        {
            Debugger.DoAssert(idx < 8, "Layout index out of bounds");
            return this.m_editModeLayoutPosition[idx];
        }

        public string GetLayoutVariableNameX(int idx, bool draf)
        {
            if (draf)
            {
                switch (idx)
                {
                    case 0: return "emx";
                    case 1: return "e1x";
                    case 2: return "e2x";
                    case 3: return "e3x";
                    case 4: return "e4x";
                    case 5: return "e5x";
                    case 6: return "e6x";
                    case 7: return "e7x";
                    default:
                        Debugger.Error("Layout index out of bounds");
                        return "emx";
                }
            }

            switch (idx)
            {
                case 0: return "lmx";
                case 1: return "l1x";
                case 2: return "l2x";
                case 3: return "l3x";
                case 4: return "l4x";
                case 5: return "l5x";
                case 6: return "l6x";
                case 7: return "l7x";
                default:
                    Debugger.Error("Layout index out of bounds");
                    return "lmx";
            }
        }

        public string GetLayoutVariableNameY(int idx, bool draft)
        {
            if (draft)
            {
                switch (idx)
                {
                    case 0: return "emy";
                    case 1: return "e1y";
                    case 2: return "e2y";
                    case 3: return "e3y";
                    case 4: return "e4y";
                    case 5: return "e5y";
                    case 6: return "e6y";
                    case 7: return "e7y";
                    default:
                        Debugger.Error("Layout index out of bounds");
                        return "emy";
                }
            }

            switch (idx)
            {
                case 0: return "lmy";
                case 1: return "l1y";
                case 2: return "l2y";
                case 3: return "l3y";
                case 4: return "l4y";
                case 5: return "l5y";
                case 6: return "l6y";
                case 7: return "l7y";
                default:
                    Debugger.Error("Layout index out of bounds");
                    return "l1x";
            }
        }

        public string GetLayoutVariableNameTrapDirection(int idx, bool draft)
        {
            if (draft)
            {
                switch (idx)
                {
                    case 0: return "trapd_draft";
                    case 1: return "trapd_draft_war";
                    case 2: return "trapd_d2";
                    case 3: return "trapd_d3";
                    case 4: return "trapd_d4";
                    case 5: return "trapd_d5";
                    case 6: return "trapd_d6";
                    case 7: return "trapd_d7";
                    default:
                        Debugger.Error("Layout index out of bounds");
                        return "trapd_draft";
                }
            }

            switch (idx)
            {
                case 0: return "trapd";
                case 1: return "trapd_war";
                case 2: return "trapd2";
                case 3: return "trapd3";
                case 4: return "trapd4";
                case 5: return "trapd5";
                case 6: return "trapd6";
                case 7: return "trapd7";
                default:
                    Debugger.Error("Layout index out of bounds");
                    return "trapd";
            }
        }

        public string GetLayoutVariableNameAirMode(int idx, bool draft)
        {
            if (draft)
            {
                switch (idx)
                {
                    case 0: return "air_mode_draft";
                    case 1: return "air_mode_draft_war";
                    case 2: return "air_mode_d2";
                    case 3: return "air_mode_d3";
                    case 4: return "air_mode_d4";
                    case 5: return "air_mode_d5";
                    case 6: return "air_mode_d6";
                    case 7: return "air_mode_d7";
                    default:
                        Debugger.Error("Layout index out of bounds");
                        return "air_mode_draft";
                }
            }

            switch (idx)
            {
                case 0: return "air_mode";
                case 1: return "air_mode_war";
                case 2: return "air_mode2";
                case 3: return "air_mode3";
                case 4: return "air_mode4";
                case 5: return "air_mode5";
                case 6: return "air_mode6";
                case 7: return "air_mode7";
                default:
                    Debugger.Error("Layout index out of bounds");
                    return "air_mode";
            }
        }

        public void SetPositionLayout(int idx, int x, int y)
        {
            Debugger.DoAssert(idx < 8, "Layout index out of bands");
            this.m_layoutPosition[idx].Set(x, y);
        }

        public void SetEditModePositionLayout(int idx, int x, int y)
        {
            Debugger.DoAssert(idx < 8, "Layout index out of bands");
            this.m_editModeLayoutPosition[idx].Set(x, y);
        }
    }
}