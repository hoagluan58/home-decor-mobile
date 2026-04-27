using UnityEngine;

namespace YoyoDesign
{
    public abstract class HomeLandSubController : MonoBehaviour
    {
        protected HomeLand _homeLand;
        protected LandBounds _bounds => _homeLand.Bounds;
        protected LandProgress _landProgress => HomeLandData.Instance.GetLandProgress(_homeLand.LandConfig.Id);

        public abstract EHomeLandControllerType Type { get; }

        public virtual void Init(HomeLand homeLand)
        {
            _homeLand = homeLand;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnRefresh()
        {

        }
    }
}
