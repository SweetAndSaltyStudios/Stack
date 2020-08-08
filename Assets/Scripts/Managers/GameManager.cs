using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sweet_And_Salty_Studios
{
    public enum GAME_STATE
    {
        WAITING,
        RUNNING,
        ENDING,
        RESTARTING
    }

    public class GameManager : Singelton<GameManager>
    {
        #region VARIABLES

        private const float MAX_MOVE_BOUNDS = 10f;
        private const float BLOCKS_MOVEMENT_SPEED = 2f;
        private const float ERROR_MARGIN = 0.25f;
        private const float BLOCK_BOUNDS_BONUS = 0.25f;
        private const int COMBO_START_LIMIT = 3;

        public Block BlockPrefab;
        public Effect EffectPrefab;
        public float DeadThreshold;
        public int BlockCount;
        public float ColorTransitionMultiplier = 250f;
        public Color32[] GameColors = new Color32[4];

        private Block[] movingBlocks;
        private int blockIndex;
        private readonly float blockMoveSpeed = 2.5f;
        private float blockTransition = 0f;
        private float secondaryPosition;

        private int combo = 0;
        private int bestScore;
        private int score;
        private int defaultScale_Y;

        private bool hasInitialized;
        private bool isMovingAlong_X_Axis;

        private Vector3 desiredPosition;
        private Vector3 previousBlockPosition;

        private Coroutine iWait_Coroutine;
        private Coroutine iRun_Coroutine;
        private Coroutine iEnd_Coroutine;
        private Coroutine iRestart_Coroutine;

        private AudioSource audioSource;

        #endregion VARIABLES

        #region PROPERTIES

        public GAME_STATE CurrentGameState
        {
            get;
            private set;
        }

        public Stack<Block> ActiveBlocks
        {
            get;
            private set;
        } = new Stack<Block>();

        private Vector2 currentMoveBounds;

        public Vector2 CurrentMoveBounds
        {
            get
            {
                return currentMoveBounds;
            }
            set
            {
                currentMoveBounds = value;
            }
        }
     
        #endregion PROPERTIES

        #region UNITY_FUNCTIONS

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            OnGameStart();
        }

        private void OnDrawGizmos()
        {
            if(CurrentGameState == GAME_STATE.RESTARTING)
            {
                return;
            }

            if(movingBlocks == null || movingBlocks.Length <= 0)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                movingBlocks[blockIndex + 1 > BlockCount - 1 ? blockIndex : blockIndex + 1].transform.position,
                movingBlocks[blockIndex].transform.localScale);
        }

        #endregion UNITY_FUNCTIONS

        #region CUSTOM_FUNCTIONS

        private void Initialize()
        {
            bestScore = PlayerPrefs.GetInt("BestScore", 0);

            defaultScale_Y = /*(int)BlockPrefab.transform.localScale.y;*/ 1;

            CreateMovingBlocks();

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;

            hasInitialized = true;
        }
       
        private void AddScore(int value)
        {
            score += value;
            UIManager.Instance.UpdateScore(score - 1);
            movingBlocks[blockIndex].PlaySfx();
        }

        private void MoveBlock()
        {
            blockTransition += blockMoveSpeed * Time.deltaTime;

            if(isMovingAlong_X_Axis)
            {
                movingBlocks[blockIndex].transform.localPosition = new Vector3(
                    Mathf.Sin(blockTransition) * MAX_MOVE_BOUNDS,
                    score,
                    secondaryPosition);
            }
            else
            {
                movingBlocks[blockIndex].transform.localPosition = new Vector3(
                    secondaryPosition,
                    score,
                    Mathf.Sin(blockTransition) * MAX_MOVE_BOUNDS);
            }
        }

        private bool TryPlaceBlock()
        {
            var currentBlockTransform = movingBlocks[blockIndex].transform;
            
            if(isMovingAlong_X_Axis)
            {
                var deltaX = previousBlockPosition.x - currentBlockTransform.position.x;

                if(Mathf.Abs(deltaX) > ERROR_MARGIN)
                {
                    combo = 0;
                    currentMoveBounds.x -= Mathf.Abs(deltaX);

                    if(currentMoveBounds.x - DeadThreshold <= 0)
                    {
                        return false;
                    }

                    var center = previousBlockPosition.x + currentBlockTransform.localPosition.x * 0.5f;

                    currentBlockTransform.localScale = new Vector3(
                        currentMoveBounds.x,
                        defaultScale_Y,
                        currentMoveBounds.y);                  

                    CreateFallingBlock(
                      new Vector3(
                      (currentBlockTransform.position.x > 0)
                      ? currentBlockTransform.position.x + (currentBlockTransform.localScale.x * 0.5f)
                      : currentBlockTransform.position.x - (currentBlockTransform.localScale.x * 0.5f),
                      currentBlockTransform.position.y,
                      currentBlockTransform.position.z),
                      new Vector3(
                          Mathf.Abs(deltaX),
                          defaultScale_Y,
                          currentBlockTransform.localScale.z
                          ));

                    currentBlockTransform.localPosition = new Vector3(
                        center - (previousBlockPosition.x * 0.5f),
                        score,
                        previousBlockPosition.z);                
                }
                else
                {
                    if(combo > COMBO_START_LIMIT)
                    {
                        currentMoveBounds.x += BLOCK_BOUNDS_BONUS;

                        if(currentMoveBounds.x > MAX_MOVE_BOUNDS)
                        {
                            currentMoveBounds.x = MAX_MOVE_BOUNDS;
                        }

                        var center = previousBlockPosition.x + currentBlockTransform.localPosition.x * 0.5f;

                        currentBlockTransform.localScale = new Vector3(
                            currentMoveBounds.x,
                            defaultScale_Y,
                            currentMoveBounds.y);

                        currentBlockTransform.localPosition = new Vector3(
                            center - (previousBlockPosition.x * 0.5f),
                            score,
                            previousBlockPosition.z);
                    }

                    ObjectPoolManager.Instance.SpawnObject(EffectPrefab, currentBlockTransform.position, Quaternion.Euler(90, 0, 0));
                    combo++;
                    currentBlockTransform.localPosition = new Vector3(
                             previousBlockPosition.x,
                             score,
                             previousBlockPosition.z);
                }
            }
            else
            {
                var deltaZ = previousBlockPosition.z - currentBlockTransform.position.z;

                if(Mathf.Abs(deltaZ) > ERROR_MARGIN)
                {
                    combo = 0;
                    currentMoveBounds.y -= Mathf.Abs(deltaZ);

                    if(currentMoveBounds.y - DeadThreshold <= 0)
                    {
                        return false;
                    }

                    var center = previousBlockPosition.z + currentBlockTransform.localPosition.z * 0.5f;

                    currentBlockTransform.localScale = new Vector3(
                        currentMoveBounds.x,
                        defaultScale_Y,
                        currentMoveBounds.y);

                    CreateFallingBlock(
                       new Vector3(currentBlockTransform.position.x,
                       currentBlockTransform.position.y,
                       (currentBlockTransform.position.z > 0)
                       ? currentBlockTransform.position.z + (currentBlockTransform.localScale.z * 0.5f)
                       : currentBlockTransform.position.z - (currentBlockTransform.localScale.z * 0.5f)),
                       new Vector3(
                           currentBlockTransform.localScale.x,
                           defaultScale_Y,
                           Mathf.Abs(deltaZ)
                           ));

                    currentBlockTransform.localPosition = new Vector3(
                        previousBlockPosition.x,
                        score,
                        center - (previousBlockPosition.z * 0.5f));                 
                }
                else
                {
                    if(combo > COMBO_START_LIMIT)
                    {
                        currentMoveBounds.y += BLOCK_BOUNDS_BONUS;

                        if(currentMoveBounds.y > MAX_MOVE_BOUNDS)
                        {
                            currentMoveBounds.y = MAX_MOVE_BOUNDS;
                        }

                        var center = previousBlockPosition.z + currentBlockTransform.localPosition.z * 0.5f;

                        currentBlockTransform.localScale = new Vector3(currentMoveBounds.x,
                            defaultScale_Y,
                            currentMoveBounds.y);

                        currentBlockTransform.localPosition = new Vector3(
                            previousBlockPosition.x,
                            score,
                            center - (previousBlockPosition.z * 0.5f));
                    }

                    ObjectPoolManager.Instance.SpawnObject(EffectPrefab, currentBlockTransform.position, Quaternion.Euler(90, 0, 0));

                    combo++;
                    currentBlockTransform.localPosition = new Vector3(
                           previousBlockPosition.x,
                           score,
                           previousBlockPosition.z);
                }
            }

            secondaryPosition = isMovingAlong_X_Axis ? currentBlockTransform.localPosition.x : currentBlockTransform.localPosition.z;

            isMovingAlong_X_Axis = !isMovingAlong_X_Axis;

            return true;
        }

        private void ReplaceBlock()
        {
            previousBlockPosition = movingBlocks[blockIndex].transform.localPosition;

            blockIndex--;

            if(blockIndex < 0)
            {
                blockIndex = movingBlocks.Length - 1;
            }

            desiredPosition = Vector3.down * score;

            movingBlocks[blockIndex].transform.localPosition = new Vector3(previousBlockPosition.x,
                score,
                previousBlockPosition.z);

            movingBlocks[blockIndex].transform.localScale = new Vector3(
                currentMoveBounds.x,
                defaultScale_Y,
                currentMoveBounds.y);

            ColorMesh(movingBlocks[blockIndex].Mesh);
        }

        private void CreateMovingBlocks()
        {
            movingBlocks = new Block[BlockCount];

            for(int i = 0; i < BlockCount; i++)
            {
                movingBlocks[i] = ObjectPoolManager.Instance.SpawnObject(BlockPrefab, new Vector3(0, i * -defaultScale_Y, 0));
                movingBlocks[i].transform.parent = transform;
                movingBlocks[i].LockBlock();

                ColorMesh(movingBlocks[i].Mesh);
            }

            blockIndex = movingBlocks.Length - 1;
        }

        private void CreateFallingBlock(Vector3 position, Vector3 scale)
        {                  
            var fallingBlock = ObjectPoolManager.Instance.SpawnObject(BlockPrefab, position);
            fallingBlock.transform.localScale = scale;
            fallingBlock.UnlockBlock();
            ColorMesh(fallingBlock.Mesh);
        }      

        private void ColorMesh(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var colors = new Color32[vertices.Length];
            var f = Mathf.Sin(score * ColorTransitionMultiplier);

            for(int i = 0; i < vertices.Length; i++)
            {
                colors[i] = LerpColors(GameColors[0], GameColors[1], GameColors[2], GameColors[3], f);
            }

            mesh.colors32 = colors;
        }

        private void OnGameStart()
        {
            UIManager.Instance.UpdateBestScore(PlayerPrefs.GetInt("BestScore"));

            ChangeGameState(GAME_STATE.WAITING);        
        }

        private void Wait()
        {
            if(iWait_Coroutine != null)
            {
                StopCoroutine(iWait_Coroutine);
            }

            iWait_Coroutine = StartCoroutine(IWaitForStart());
        }

        private void Run()
        {
            if(iRun_Coroutine != null)
            {
                StopCoroutine(iRun_Coroutine);
            }

            iRun_Coroutine = StartCoroutine(IRun());
        }

        private void End()
        {
            if(iEnd_Coroutine != null)
            {
                StopCoroutine(iEnd_Coroutine);
            }

            iEnd_Coroutine = StartCoroutine(IEndGame());
        }

        private void Restart()
        {
            if(iRestart_Coroutine != null)
            {
                StopCoroutine(iRestart_Coroutine);
            }

            iRestart_Coroutine = StartCoroutine(IRestart());
        }

        private void ChangeGameState(GAME_STATE newGameState)
        {
            CurrentGameState = newGameState;

            switch(CurrentGameState)
            {
                case GAME_STATE.WAITING:

                    Wait();

                    break;

                case GAME_STATE.RUNNING:

                    Run();

                    break;

                case GAME_STATE.ENDING:

                    End();

                    break;

                case GAME_STATE.RESTARTING:

                    Restart();

                    break;

                default:

                    break;
            }
        }

        private Color32 LerpColors(Color32 a, Color32 b, Color32 c, Color32 d, float t)
        {
            if(t < 0.33f)
            {
                return Color.Lerp(a, b, t / 0.33f);
            }
            else if(t < 0.66f)
            {
                return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
            }
            else
            {
                return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
            }
        }

        private IEnumerator IWaitForStart()
        {
            score = 0;

            yield return new WaitUntil(() => hasInitialized);

            UIManager.Instance.Fade(false, 0, 0.12f);

            currentMoveBounds = new Vector2(MAX_MOVE_BOUNDS, MAX_MOVE_BOUNDS);

            yield return new WaitUntil(() => InputManager.Instance.IsMouseUp(0));

            AddScore(1);

            ChangeGameState(GAME_STATE.RUNNING);
        }

        private IEnumerator IRun()
        {
            audioSource.Play();

            while(CurrentGameState == GAME_STATE.RUNNING)
            {
                if(InputManager.Instance.IsMouseDown(0))
                {
                    if(TryPlaceBlock())
                    {
                        ReplaceBlock();
                        AddScore(defaultScale_Y);
                    }
                    else
                    {
                        ChangeGameState(GAME_STATE.ENDING);
                        break;
                    }
                }

                MoveBlock();

                transform.position = Vector3.Lerp(transform.position, desiredPosition, BLOCKS_MOVEMENT_SPEED * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator IEndGame()
        {
            if(score > bestScore)
            {
                PlayerPrefs.SetInt("BestScore", score);
            }

            audioSource.Stop();

            CameraEngine.Instance.ShakeCamera();

            movingBlocks[blockIndex].UnlockBlock();

            movingBlocks = movingBlocks.OrderBy(block => block.transform.localPosition.y).ToArray();    

            for(int i = 0; i < movingBlocks.Length - 1; i++)
            {               
                blockIndex = i;
                movingBlocks[i].UnlockBlock();
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitUntil(() => ActiveBlocks.Count <= 0);

            ChangeGameState(GAME_STATE.RESTARTING);
        }

        private IEnumerator IRestart()
        {
            UIManager.Instance.Fade(true, 1, 2);

            yield return new WaitForSeconds(2);

            SceneManager.LoadScene(0);

            yield return null;
        }

        #endregion CUSTOM_FUNCTIONS
    }
}