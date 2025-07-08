using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace MultiplayerGames
{
    namespace CardHarmony{
        public class CardController : MonoBehaviour
        {
            public int index = -1;
            public int type;
            public void FlipDown(){
                transform.DOKill();
                transform.DOScale(1.2f, 0.15f).OnComplete(() => {
                    transform.DORotate(transform.eulerAngles.alterMember(y:180f), 0.15f).OnComplete(() => {
                        transform.DOScale(1f, 0.15f);
                    });
                });
            }
            public void FlipUp(){
                transform.DOKill();
                transform.DOScale(1.2f, 0.15f).OnComplete(() => {
                    transform.DORotate(transform.eulerAngles.alterMember(y:0f), 0.15f).OnComplete(() => {
                        transform.DOScale(1f, 0.15f);
                    });
                });
            }
            public void FlipUpImmediate()
            {
                transform.DOKill();
                Vector3 targetRotation = transform.eulerAngles;
                targetRotation.y = 0f;
                transform.eulerAngles = targetRotation;
                transform.localScale = Vector3.one;
            }

        }
    }
}