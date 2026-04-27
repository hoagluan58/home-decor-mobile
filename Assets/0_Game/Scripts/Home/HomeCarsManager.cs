using DG.Tweening;
using NFramework;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace YoyoDesign
{
    public class HomeCarsManager : MonoBehaviour
    {
        [SerializeField] private List<SkeletonAnimation> _cars;
        [SerializeField] private List<string> _carSkinNames;

        private void OnDisable()
        {
            this.DOKill();
        }

        private void Start()
        {
            var doneTutorial = UserData.Instance.CurTutorialIndex != Define.TutorialIndex.DONE_INTRO;
            if (!doneTutorial)
                return;

            PlayRandomTraffic();
            DOVirtual.DelayedCall(12, PlayRandomTraffic)
                .SetLoops(-1)
                .SetTarget(this);
        }

        private void PlayRandomTraffic()
        {
            var array = Enum.GetValues(typeof(EHomeCarsSequence));
            var randomValue = (EHomeCarsSequence)array.GetValue(Random.Range(0, array.Length));
            PlayTraffic(randomValue);
        }

        private void PlayTraffic(EHomeCarsSequence key)
        {
            switch (key)
            {
                case EHomeCarsSequence.First:
                    {
                        var car1 = GetCar();
                        var car1Transform = car1.transform;
                        var car1Sequence = DOTween.Sequence();
                        car1Sequence
                            .AppendCallback(() =>
                            {
                                car1Transform.localPosition = new Vector3(-7, 55);
                                car1.AnimationState.SetAnimation(0, "car_run1", true);
                            })
                            .Append(car1Transform.DOLocalMove(new Vector2(17, 40), 3).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-20, 15), 3).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run1", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(29, -17), 3).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-46, -63), 5).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.gameObject.SetActive(false));
                        car1Sequence.SetTarget(this);
                        car1Sequence.Play();

                        var car2 = GetCar();
                        var car2Transform = car2.transform;
                        var car2Sequence = DOTween.Sequence();
                        car2Sequence
                            .AppendCallback(() =>
                            {
                                car2Transform.localPosition = new Vector3(-72, 46);
                                car2.AnimationState.SetAnimation(0, "car_run1", true);
                            })
                            .Append(car2Transform.DOLocalMove(new Vector2(-23, 17), 5).SetEase(Ease.Linear))
                            .AppendCallback(() => car2.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car2Transform.DOLocalMove(new Vector2(-71, -14), 4).SetEase(Ease.Linear))
                            .AppendCallback(() => car2.gameObject.SetActive(false));

                        car2Sequence.SetTarget(this);
                        car2Sequence.Play();
                        break;
                    }

                case EHomeCarsSequence.Second:
                    {
                        var car1 = GetCar();
                        var car1Transform = car1.transform;
                        var car1Sequence = DOTween.Sequence();
                        car1Sequence
                            .AppendCallback(() =>
                            {
                                car1Transform.localPosition = new Vector3(44, 55);
                                car1.AnimationState.SetAnimation(0, "car_run2", true);
                            })
                            .Append(car1Transform.DOLocalMove(new Vector2(-71, -14), 8).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.gameObject.SetActive(false));

                        car1Sequence.SetTarget(this);
                        car1Sequence.Play();

                        var car2 = GetCar();
                        var car2Transform = car2.transform;
                        var car2Sequence = DOTween.Sequence();
                        car2Sequence
                            .AppendCallback(() =>
                            {
                                car2Transform.localPosition = new Vector3(71, 40);
                                car2.AnimationState.SetAnimation(0, "car_run2", true);
                            })
                            .Append(car2Transform.DOLocalMove(new Vector2(4, 2), 4).SetEase(Ease.Linear))
                            .AppendCallback(() => car2.AnimationState.SetAnimation(0, "car_run1", true))
                            .Append(car2Transform.DOLocalMove(new Vector2(29, -17), 2).SetEase(Ease.Linear))
                            .AppendCallback(() => car2.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car2Transform.DOLocalMove(new Vector2(-46, -63), 4).SetEase(Ease.Linear))
                            .AppendCallback(() => car2.gameObject.SetActive(false));

                        car2Sequence.SetTarget(this);
                        car2Sequence.Play();
                        break;
                    }
                case EHomeCarsSequence.Third:
                    {
                        var car1 = GetCar();
                        var car1Transform = car1.transform;
                        var car1Sequence = DOTween.Sequence();
                        car1Sequence
                            .AppendCallback(() =>
                            {
                                car1Transform.localPosition = new Vector3(-72, 46);
                                car1.AnimationState.SetAnimation(0, "car_run1", true);
                            })
                            .Append(car1Transform.DOLocalMove(new Vector2(-22, 15), 3).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-33, 8), 1).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run1", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-6, -9), 2).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-36, -28), 2).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run1", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-15, -42), 2).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.AnimationState.SetAnimation(0, "car_run2", true))
                            .Append(car1Transform.DOLocalMove(new Vector2(-46, -63), 2).SetEase(Ease.Linear))
                            .AppendCallback(() => car1.gameObject.SetActive(false));

                        car1Sequence.SetTarget(this);
                        car1Sequence.Play();
                        break;
                    }
                case EHomeCarsSequence.Fourth:
                    {
                        var car2 = GetCar();
                        var car2Transform = car2.transform;
                        var car2Sequence = DOTween.Sequence();
                        car2Sequence
                            .AppendCallback(() =>
                            {
                                car2Transform.localPosition = new Vector3(71, 7);
                                car2.AnimationState.SetAnimation(0, "car_run2", true);
                            })
                            .Append(car2Transform.DOLocalMove(new Vector2(-46, -63), 7).SetEase(Ease.Linear))
                            .AppendCallback(() => car2.gameObject.SetActive(false));

                        car2Sequence.SetTarget(this);
                        car2Sequence.Play();
                        break;
                    }
            }
        }

        private SkeletonAnimation GetCar()
        {
            var carGet = _cars.FirstOrDefault(c => !c.gameObject.activeSelf);
            if (carGet == null) return carGet;
            carGet.gameObject.SetActive(true);
            carGet.Skeleton.SetSkin(_carSkinNames.RandomItem());
            carGet.Skeleton.SetSlotsToSetupPose();
            carGet.LateUpdate();
            return carGet;
        }
    }

    public enum EHomeCarsSequence
    {
        First,
        Second,
        Third,
        Fourth,
    }
}