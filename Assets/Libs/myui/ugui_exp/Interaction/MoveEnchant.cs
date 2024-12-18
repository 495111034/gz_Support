using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

public class MoveEnchant : UIBehaviour, 
    IPointerClickHandler,
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler, 
    IPointerEnterHandler,
    IPointerDownHandler
{
    public Action<Transform, PointerEventData> OnEnchantBeginDrag;
    public Action<PointerEventData> OnEnchantDrag;
    public Action<Transform, PointerEventData> OnEnchantEndDrag;
    public Action<Transform, PointerEventData> OnEnchantClick;
    public Action<Transform> OnClick;
    public Action<Transform, PointerEventData> OnEnchantEnter;
    public Action<Transform, PointerEventData> OnEnchantDown;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Transform _transform = transform;
        OnEnchantBeginDrag?.Invoke(_transform, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnEnchantDrag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Transform _transform = transform;
        OnEnchantEndDrag?.Invoke(_transform, eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Transform _transform = transform;
        OnEnchantClick?.Invoke(_transform, eventData);
        OnClick?.Invoke(_transform);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Transform _transform = transform;
        OnEnchantDown?.Invoke(_transform, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Transform _transform = transform;
        OnEnchantEnter?.Invoke(_transform, eventData);
    }
}
