using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;

namespace MultiplayerGames
{
    namespace CardHarmony
    {
        public class CardHarmonyLM : MonoBehaviour
        {
            [Header("Gameplay Settings")] private int cols, rows;
            [SerializeField] float spacing = 1.2f;
            [SerializeField] Vector2 cardSize = new Vector2(0.65f, 0.65f);

            private List<Vector2Int> boardSizes = new List<Vector2Int>
            {
                new Vector2Int(2, 2),
                new Vector2Int(2, 3),
                new Vector2Int(3, 4),
                new Vector2Int(4, 4),
                new Vector2Int(4, 5),
                new Vector2Int(5, 6),
                new Vector2Int(6, 6),
                new Vector2Int(6, 7),
                new Vector2Int(8, 8),
            };

            private int currentBoardIndex = 0;

            [Header("Audio")] [SerializeField] AudioSource pixieDustSound, tickSound;

            [Header("Card Prefabs")] [SerializeField]
            GameObject[] prefabCards;

            [Header("UI")] [SerializeField] private TextMeshProUGUI txtBlueScore;
            [SerializeField] private TextMeshProUGUI txtBestScore;
            [Header("Timing")] [SerializeField] private float flashingTime = 0.3f, showTime = 0.7f, yieldTime = 0.2f;

            private CardController[] cards;
            private bool[] cardShowing;
            private HashSet<int> cardIndexesOnTheTable;
            public int matchedPairs = 0;
            private bool isPlaying;
            private int card1Index = -1;
            private int p1Score = 0;
            private int bestScore = 0;
            private float coolDown = -1f;
            private LayerMask characterMask;
            private Transform cardParent;
            string SavePath => Application.persistentDataPath + "/card_harmony_save.json";

            void Start()
            {
                characterMask = LayerMask.GetMask("Characters");
                bestScore = PlayerPrefs.GetInt("CardHarmony_BestScore", 0);
                txtBestScore.text = $"Best Score: {bestScore}";
                isPlaying = true;
            }

            public void SpawnCardGrid()
            {
                // Dọn card cũ nếu có
                if (cardParent != null)
                    Destroy(cardParent.gameObject);

                cardParent = new GameObject("CardParent").transform;
                cardParent.SetParent(transform);
                cardParent.localPosition = Vector3.zero;
                matchedPairs = 0;
                Vector2Int size = boardSizes[currentBoardIndex];
                rows = size.y;
                cols = size.x;
                int totalCards = size.y * size.x;

                if (totalCards % 2 != 0)
                {
                    Debug.LogError("Tổng số ô phải là số chẵn để chia cặp!");
                    return;
                }

                // Tính kích thước từ prefab
                DetectCardSizeFromPrefab();

                // Tính kích thước toàn lưới
                float totalWidth = cols * cardSize.x + (cols - 1) * spacing;
                float totalHeight = rows * cardSize.y + (rows - 1) * spacing;
                
                FitCameraToGrid(totalWidth, totalHeight);

                //  Tính vị trí bắt đầu để canh giữa
                Vector3 origin = new Vector3(-totalWidth / 2 + cardSize.x / 2, -totalHeight / 2 + cardSize.y / 2, 0f);

                //  Sinh các cặp ngẫu nhiên
                int pairCount = totalCards / 2;
                List<int> spawnTypes = new List<int>();
                for (int i = 0; i < pairCount; i++)
                {
                    int rand = Random.Range(0, prefabCards.Length);
                    spawnTypes.Add(rand);
                    spawnTypes.Add(rand);
                }

                spawnTypes = spawnTypes.OrderBy(x => Random.value).ToList();
                
                cards = new CardController[totalCards];
                cardShowing = new bool[totalCards];
                cardIndexesOnTheTable = new HashSet<int>();
                
                int index = 0;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int type = spawnTypes[index];
                        Vector3 pos = origin + new Vector3(c * (cardSize.x + spacing), r * (cardSize.y + spacing), 0f);

                        GameObject go = Instantiate(prefabCards[type], pos, Quaternion.Euler(0, 180, 0), cardParent);
                        go.transform.localScale = Vector3.one;
                        int idx = index;

                        go.transform.PassTo(t =>
                        {
                            cards[idx] = t.GetComponent<CardController>();
                            cards[idx].index = idx;
                        });

                        cardShowing[index] = false;
                        cardIndexesOnTheTable.Add(index);
                        index++;
                    }
                }

                SaveData();
            }

            void DetectCardSizeFromPrefab()
            {
                if (prefabCards.Length == 0) return;

                GameObject prefab = prefabCards[0];

                if (prefab.TryGetComponent<Renderer>(out var rend))
                {
                    cardSize = rend.bounds.size;
                }
                else if (prefab.TryGetComponent<BoxCollider2D>(out var col))
                {
                    cardSize = col.size;
                }
                else if (prefab.TryGetComponent<RectTransform>(out var rect))
                {
                    cardSize = rect.rect.size;
                }
                else
                {
                    Debug.LogWarning("Không thể xác định kích thước card từ prefab.");
                }
            }

            void FitCameraToGrid(float gridWidth, float gridHeight)
            {
                float screenAspect = (float)Screen.width / Screen.height;

                float verticalSize = gridHeight / 2f + 1f; // thêm biên
                float horizontalSize = (gridWidth / 2f) / screenAspect + 1f;

                Camera.main.orthographicSize = Mathf.Max(5f, verticalSize, horizontalSize);
            }


            void PointToCurrentPlayer()
            {
                ++p1Score;
                txtBlueScore.text = "Score: " + p1Score;

                ++matchedPairs;
                int maxPairs = (rows * cols) / 2;

                if (p1Score > bestScore)
                {
                    bestScore = p1Score;
                    PlayerPrefs.SetInt("CardHarmony_BestScore", bestScore);
                    txtBestScore.text = $"Best Score: {bestScore}";
                }

                if (matchedPairs >= maxPairs)
                {
                    if (currentBoardIndex < boardSizes.Count - 1)
                        currentBoardIndex++;

                    this.SetTimeout(SpawnCardGrid, 2.3f);
                }

                SaveData(); // thêm dòng này
            }


            void Hit(CardController cardController)
            {
                if (!cardShowing[cardController.index])
                {
                    cardController.FlipUp();
                    AudioManager.Instance.PlayAudio(tickSound);
                    cardShowing[cardController.index] = true;

                    if (card1Index < 0)
                    {
                        card1Index = cardController.index;
                        coolDown = 0.45f;
                    }
                    else
                    {
                        int i = card1Index;
                        int j = cardController.index;

                        if (cards[i].type == cards[j].type)
                        {
                            PointToCurrentPlayer();
                            cardIndexesOnTheTable.Remove(i);
                            cardIndexesOnTheTable.Remove(j);
                            SaveData(); // thêm dòng này
                            this.SetTimeout((() =>
                            {
                                cards[i].transform.DOMove(new Vector3(0, 1.3f, -6f), 0.15f);
                                cards[i].transform.DOScale(1.5f, 0.15f).OnComplete(() =>
                                {
                                    cards[i].transform.DOMove(new Vector3(-20f, 1.3f, -6f), 0.6f)
                                        .OnComplete(() => Destroy(cards[i].gameObject)).SetDelay(1f);
                                });

                                cards[j].transform.DOMove(new Vector3(0, -1.3f, -6f), 0.15f);
                                cards[j].transform.DOScale(1.5f, 0.15f).OnComplete(() =>
                                {
                                    cards[j].transform.DOMove(new Vector3(20f, -1.3f, -6f), 0.6f)
                                        .OnComplete(() =>
                                        {
                                            Destroy(cards[j].gameObject);
                                        }).SetDelay(1f);
                                });

                                AudioManager.Instance.PlayAudio(pixieDustSound);
                            }), 0.45f);

                            coolDown = 2.3f;
                            card1Index = -1;
                        }
                        else
                        {
                            this.SetTimeout((() =>
                            {
                                cards[i].FlipDown();
                                cards[j].FlipDown();
                                cardShowing[i] = false;
                                cardShowing[j] = false;
                            }), 0.45f);

                            coolDown = 1f;
                            card1Index = -1;
                        }
                    }
                }
            }

            void PointerDown(Vector2 pos)
            {
                if (coolDown < 0)
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
                    RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector3.forward, 20f, characterMask);

                    if (hit.collider == null)
                        return;

                    CardController cc = hit.collider.GetComponent<CardController>();
                    if (cc != null)
                        Hit(cc);
                }
            }

            void Update()
            {
                if (!isPlaying) return;

                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; ++i)
                        PointerDown(Input.GetTouch(i).position);
                }
                else if (Input.GetMouseButton(0))
                {
                    PointerDown(Input.mousePosition);
                }

                coolDown -= Time.deltaTime;
            }

            public void OnClickHome()
            {
                GameManager.Instance.ReturnToMenu();
                GameManager.Instance.CheckStateButton();
            }

            void SaveData()
            {
                var saveData = new CardHarmonySaveData
                {
                    currentBoardIndex = currentBoardIndex,
                    bestScore = bestScore,
                    p1Score = p1Score,
                    matchedPairs = matchedPairs,
                    cardTypes = cards.Where(c => c != null).Select(c => c.type).ToList(),
                    cardIndexesOnTheTable = cardIndexesOnTheTable.ToList(), // ⚠️ Chỉ những cái chưa bị ăn
                    cardShowing = cardShowing.ToList()
                };

                string json = JsonUtility.ToJson(saveData);
                File.WriteAllText(SavePath, json);
                Debug.Log("Game Saved");
            }


            public void LoadFromSavedGame()
            {
                if (!File.Exists(SavePath))
                {
                    Debug.LogWarning("Không tìm thấy save file. Bắt đầu ván mới.");

                    return;
                }

                string json = File.ReadAllText(SavePath);
                var saveData = JsonUtility.FromJson<CardHarmonySaveData>(json);

                currentBoardIndex = saveData.currentBoardIndex;
                bestScore = saveData.bestScore;
                p1Score = saveData.p1Score;
                matchedPairs = saveData.matchedPairs;

                txtBestScore.text = $"Best Score: {bestScore}";
                txtBlueScore.text = "Score: " + p1Score;

                SpawnCardGridWithSave(saveData);
                isPlaying = true;
            }


            void SpawnCardGridWithSave(CardHarmonySaveData saveData)
            {
                if (cardParent != null)
                    Destroy(cardParent.gameObject);

                cardParent = new GameObject("CardParent").transform;
                cardParent.SetParent(transform);
                cardParent.localPosition = Vector3.zero;

                Vector2Int size = boardSizes[saveData.currentBoardIndex];
                rows = size.y;
                cols = size.x;
                int totalCards = rows * cols;

                cardIndexesOnTheTable = new HashSet<int>(saveData.cardIndexesOnTheTable);
                cards = new CardController[totalCards];
                cardShowing = saveData.cardShowing.ToArray();

                DetectCardSizeFromPrefab();

                float totalWidth = cols * cardSize.x + (cols - 1) * spacing;
                float totalHeight = rows * cardSize.y + (rows - 1) * spacing;
                FitCameraToGrid(totalWidth, totalHeight);

                Vector3 origin = new Vector3(-totalWidth / 2 + cardSize.x / 2, -totalHeight / 2 + cardSize.y / 2, 0f);

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int index = r * cols + c;
                        
                        if (!cardIndexesOnTheTable.Contains(index))
                            continue;

                        int type = saveData.cardTypes[index];
                        Vector3 pos = origin + new Vector3(c * (cardSize.x + spacing), r * (cardSize.y + spacing), 0f);

                        GameObject go = Instantiate(prefabCards[type], pos, Quaternion.Euler(0, 180, 0), cardParent);
                        go.transform.localScale = Vector3.one;

                        int idx = index;
                        go.transform.PassTo(t =>
                        {
                            cards[idx] = t.GetComponent<CardController>();
                            cards[idx].index = idx;
                            cards[idx].type = type;

                            if (cardShowing[idx])
                                cards[idx].FlipUpImmediate();
                        });
                    }
                }

                matchedPairs = p1Score;
            }
        }
    }
}