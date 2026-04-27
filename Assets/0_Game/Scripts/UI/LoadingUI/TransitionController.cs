using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionController : SingletonMono<TransitionController>
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private List<Color> _listColors;

    private Canvas _canvas;
    private readonly int _circleSizeId = Shader.PropertyToID("_Circle_Size");
    private readonly int _backgroundColorId = Shader.PropertyToID("_Background");

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
    }

    public async UniTask Open()
    {
        _canvas.enabled = true;
        _backgroundImage.materialForRendering.SetColor(_backgroundColorId, _listColors[Random.Range(0, _listColors.Count)]);
        _backgroundImage.materialForRendering.SetFloat(_circleSizeId, 1);

        await DOVirtual.Float(1, 0, Define.TimeLength.LOADING_ANIM, value => _backgroundImage.materialForRendering.SetFloat(_circleSizeId, value));
    }

    public async UniTask Close()
    {
        await DOVirtual.Float(0, 1, Define.TimeLength.LOADING_ANIM * 2f,
            value => _backgroundImage.materialForRendering.SetFloat(_circleSizeId, value));
        _canvas.enabled = false;
    }
}