namespace Supercell.Magic.Logic.GameObject.Listener
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Titan.Math;

    public class LogicGameObjectListener
    {
        public virtual void Destruct()
        {
            // Destruct.
        }

        public virtual void RefreshPositionFromLogic()
        {
            // RefreshPositionFromLogic.
        }

        public virtual void RefreshState()
        {
            // RefreshState.
        }

        public virtual void RandomizeStartingFrame()
        {
            // RandomizeStartingFrame.
        }

        public virtual void Damaged()
        {
            // Damaged.
        }

        public virtual void ResourcesCollected(LogicResourceData data, int count, bool unk)
        {
            // ResourcesCollected.
        }

        public virtual void RefreshResourceCount()
        {
            // RefreshResourceCount.
        }

        public virtual void XpGained(int count)
        {
            // XpGained.
        }

        public virtual void LoadedFromJSON()
        {
            // XpGained.
        }

        public virtual void MapUnlocked()
        {
            // MapUnlocked.
        }

        public virtual void ExtraCharacterAdded(LogicCharacterData character, LogicBuilding baseBuilding)
        {
            // ExtraCharacterAdded.
        }

        public virtual void CancelNotification()
        {
            // CancelNotification.
        }

        public virtual void UnitRemoved(LogicCombatItemData data)
        {
            // UnitRemoved.
        }

        public virtual void PlayEffect(LogicEffectData data)
        {
            // PlayEffect.
        }

        public virtual void PlayEffect(LogicEffectData data, int offsetX, int offsetY)
        {
            // PlayEffect.
        }

        public virtual void PlayTargetedEffect(LogicEffectData data, LogicGameObject gameObject, LogicVector2 target)
        {
            // PlayTargetedEffect.
        }

        public virtual void SpottedEnemy()
        {
            // SpottedEnemy.
        }
    }
}