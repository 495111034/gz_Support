using GameSupport;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleGrass
{
  [RequireComponent(typeof(BoxCollider))]    
  public class SimpleGrassTraceEffectTrigger : BasicBoxTrigger
    {
        public const int KindCode = 1; 
        public static void Register(UnityAction<Transform, TRIGGER_STATUS> action)
        {
            GameSupport.BasicBoxTrigger.RegisterKind(KindCode,action);
        }

        public static void UnRegister()
        {
            GameSupport.BasicBoxTrigger.UnRegisterKind(KindCode);
        }

        protected override bool CheckValid()
        {
            return SimpleGrassTrace.S_HeroTraceEnabled;
        }

        protected override int RegisterCode()
        {
            return KindCode;
        }

        protected override bool IsInCollider()
        {            
            return base._Contains(SimpleGrassTrace.S_HeroFootPos); 
        }

        //override protected void Update()
        //{
        //    base.Update();
        // }
    }
}

