using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NaturalDisaster : MonoBehaviour
{
    public void dayEffect(int day) { 
        switch (day) {
            case 1:
                day1Effect();
                break;
            case 2:
                day2Effect();
                break;
            case 3:
                day3Effect();
                break;
            case 4:
                day4Effect();
                break;
            default:
                break;
        }
    }
    protected abstract void day1Effect();
    protected abstract void day2Effect();
    protected abstract void day3Effect();
    protected abstract void day4Effect();
}
