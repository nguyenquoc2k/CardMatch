using System;
using System.Collections.Generic;

[Serializable]
public class CardHarmonySaveData
{
    public int currentBoardIndex;
    public int bestScore;
    public int p1Score;

    public List<int> cardTypes;
    public List<int> cardIndexesOnTheTable;
    public List<bool> cardShowing;
    public int matchedPairs; 

}