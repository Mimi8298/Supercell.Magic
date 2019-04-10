namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class ChangeGameStateMessage : ServerSessionMessage
    {
        // -- HOME STATE --
        public int LayoutId { get; set; }
        public int MapId { get; set; }

        // -- NPC ATTACK/DUEL STATE --
        public GameStateType StateType { get; set; }
        public LogicNpcData NpcData { get; set; }

        // -- VISIT --
        public LogicLong HomeId { get; set; }
        public int VisitType { get; set; }

        // -- CHALLENGE --
        public LogicLong ChallengeHomeId { get; set; }
        public LogicLong ChallengeStreamId { get; set; }
        public LogicLong ChallengeAllianceId { get; set; }
        public byte[] ChallengeHomeJSON { get; set; }
        public int ChallengeMapId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt((int) this.StateType);

            switch (this.StateType)
            {
                case GameStateType.HOME:
                    stream.WriteVInt(this.LayoutId);
                    stream.WriteVInt(this.MapId);
                    break;
                case GameStateType.NPC_ATTACK:
                case GameStateType.NPC_DUEL:
                    ByteStreamHelper.WriteDataReference(stream, this.NpcData);
                    break;
                case GameStateType.VISIT:
                    stream.WriteLong(this.HomeId);
                    stream.WriteVInt(this.VisitType);
                    break;
                case GameStateType.CHALLENGE_ATTACK:
                    stream.WriteLong(this.ChallengeHomeId);
                    stream.WriteLong(this.ChallengeStreamId);
                    stream.WriteLong(this.ChallengeAllianceId);
                    stream.WriteBytes(this.ChallengeHomeJSON, this.ChallengeHomeJSON.Length);
                    stream.WriteVInt(this.ChallengeMapId);
                    break;
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.StateType = (GameStateType) stream.ReadVInt();

            switch (this.StateType)
            {
                case GameStateType.HOME:
                    this.LayoutId = stream.ReadVInt();
                    this.MapId = stream.ReadVInt();
                    break;
                case GameStateType.NPC_ATTACK:
                case GameStateType.NPC_DUEL:
                    this.NpcData = (LogicNpcData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.NPC);
                    break;
                case GameStateType.VISIT:
                    this.HomeId = stream.ReadLong();
                    this.VisitType = stream.ReadVInt();
                    break;
                case GameStateType.CHALLENGE_ATTACK:
                    this.ChallengeHomeId = stream.ReadLong();
                    this.ChallengeStreamId = stream.ReadLong();
                    this.ChallengeAllianceId = stream.ReadLong();
                    this.ChallengeHomeJSON = stream.ReadBytes(stream.ReadBytesLength(), 900000);
                    this.ChallengeMapId = stream.ReadVInt();
                    break;
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CHANGE_GAME_STATE;
        }
    }
}