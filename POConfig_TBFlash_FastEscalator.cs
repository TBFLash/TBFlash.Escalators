using System;
using UnityEngine;

namespace TBFlash.Escalators
{
    public class POConfig_TBFlash_FastEscalator : POConfig_NoData<TBFlash_FastEscalator>
    {
        public override Type RuntimeType()
        {
            return typeof(TBFlash_FastEscalator);
        }
        public bool isBidirectional;
        public POConfig_Component ComponentConfig;
        public int PLACEMENT_OFFSET;
        public Sprite lightSprite;
    }
}
