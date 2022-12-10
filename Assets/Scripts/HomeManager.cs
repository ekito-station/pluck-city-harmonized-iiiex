using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class HomeManager : MonoBehaviourPunCallbacks
{
    // 位置合わせ用
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private ARTrackedImageManager imageManager;
    private GameObject worldOrigin;    // ワールドの原点として振る舞うオブジェクト
    private Coroutine originCoroutine;

    public GameObject arCamera;
    public GameObject myStrPrefab;
    public GameObject othersStrPrefab;

    public float updatePitchInterval = 0.1f; // strPitchの値を更新する間隔
    private WaitForSeconds waitToUpdate;

    private int count;
    private Vector3 markerPos;

    public float strRadius = 0.5f;

    public float minDist = 0f;    // 2個目のピンを置くまでに最低限移動する距離
    public float maxRatio = 4.0f;   // 弦が張られる自分のピンと他人のピンの最大距離^2 = maxRatio * sqrInitLength
    public float minRatio = 0.28f;  // 弦が張られる自分のピンと他人のピンの最小距離^2 = minRatio * sqrInitLength
    private float[] harmonies = { 0.36f, 0.44f, 0.56f, 0.64f, 1.0f, 1.44f, 1.78f, 2.25f, 2.56f, 3.16f, 100.0f };

    public GameObject expText;
    public Text pitchText;
    public float front;
    public GameObject checkTracking;

    private float sqrInitLength;
    private float strPitch;

    // 位置合わせ
    private void OnEnable()
    {
        worldOrigin = new GameObject("Origin");
        Debug.Log("Created origin.");
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private IEnumerator OriginDecide(ARTrackedImage trackedImage, float trackInterval)
    {
        yield return new WaitForSeconds(trackInterval);
        var trackedImageTransform = trackedImage.transform;
        worldOrigin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        sessionOrigin.MakeContentAppearAt(worldOrigin.transform, trackedImageTransform.position, trackedImageTransform.localRotation);
        Debug.Log("Adjusted the origin.");
        originCoroutine = null;
        checkTracking.SetActive(false);
    }

    // ワールド座標を任意の点から見たローカル座標に変換
    public Vector3 WorldToOriginLocal(Vector3 world)    // worldはワールド座標
    {
        return worldOrigin.transform.InverseTransformDirection(world);
    }

    // TrackedImagesChanged時の処理
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)  // eventArgsは検出イベントに関する引数
    {
        foreach (var trackedImage in eventArgs.added)
        {
            checkTracking.SetActive(true);
            StartCoroutine(OriginDecide(trackedImage, 0));
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (originCoroutine == null)
            {
                checkTracking.SetActive(true);
                originCoroutine = StartCoroutine(OriginDecide(trackedImage, 0.5f));
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 初期化
        waitToUpdate = new WaitForSeconds(updatePitchInterval);
        count = 0;

        // strPitchの値を更新するコルーチンを開始
        StartCoroutine(UpdatePitch());
        Debug.Log("Started UpdatePitch Coroutine.");
    }

    private IEnumerator UpdatePitch()
    {
        while (true)
        {
            if (count > 0)
            {
                Transform camTran = arCamera.transform;
                Vector3 curPos = camTran.position + front * camTran.forward;
                float sqrDist = (curPos - markerPos).sqrMagnitude;
                if (count == 1) // ピンが1個の場合
                {
                    if (sqrDist > minDist)
                    {
                        expText.SetActive(true);
                        pitchText.text = "C3";                        
                    }
                }
                else    // ピンが2個以上の場合
                {
                    float ratio = sqrDist / sqrInitLength;
                    foreach (float harmony in harmonies)
                    {
                        if (ratio < harmony)
                        {
                            // strPitchの値を更新
                            strPitch = harmony;
                            pitchText.text = strPitch.ToString();
                            switch (strPitch)
                            {
                                case 0.36f:
                                    pitchText.text = "B3";
                                    break; 
                                case 0.44f:
                                    pitchText.text = "A3";                                    
                                    break;                
                                case 0.56f:
                                    pitchText.text = "G3";                                    
                                    break;
                                case 0.64f:
                                    pitchText.text = "F3";                                    
                                    break;
                                case 1.0f:
                                    pitchText.text = "E3";                                    
                                    break;
                                case 1.44f:
                                     pitchText.text = "C3";                                   
                                    break;
                                case 1.78f:
                                    pitchText.text = "A2";                                    
                                    break;
                                case 2.25f:
                                    pitchText.text = "G2";                                    
                                    break;
                                case 2.56f:
                                    pitchText.text = "F2";                                    
                                    break;
                                case 3.16f:
                                    pitchText.text = "E2";                                    
                                    break;
                                case 100.0f:
                                    pitchText.text = "D2";                                    
                                    break;                                                                                                                                            
                                default:
                                    pitchText.text = ""; 
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
            yield return waitToUpdate;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickMarkerButton()
    {
        Debug.Log("Clicked Marker Button.");

        // ピンを配置
        Transform camTran = arCamera.transform;
        Vector3 curPos = camTran.position + front * camTran.forward;
        PhotonNetwork.Instantiate("Marker1", curPos, Quaternion.Euler(90f, 0f, 0f));
        Debug.Log("Placed a marker.");
        // 2つ目以降のピンなら弦を張る
        if (count > 0)
        {
            // 弦をコライダーと共に設置
            Vector3 strVec = curPos - markerPos;    // 弦の方向を取得
            float dist = strVec.magnitude;  // 弦の長さを取得
            Vector3 strY = new Vector3(0f, dist, 0f);
            Vector3 strVec2 = strVec * 0.5f;
            Vector3 centerCoord = markerPos + strVec2;  // 中点の座標        

            GameObject str = Instantiate(myStrPrefab, centerCoord, Quaternion.identity) as GameObject;
            str.transform.localScale = new Vector3(strRadius, dist / 2, strRadius); // ひとまずY軸方向に伸ばす

            CapsuleCollider col = str.GetComponent<CapsuleCollider>();
            col.isTrigger = true;   // 衝突判定を行わないように
            str.transform.rotation = Quaternion.FromToRotation(strY, strVec);   // 弦を回転

            // 弦にプロパティ(pitch)を追加
            StringController stringController = str.GetComponent<StringController>();
            if (count == 1)
            {
                // 最初に張られた弦の場合、弦の長さを保存しドを割り当てる
                sqrInitLength = (curPos - markerPos).sqrMagnitude;
                stringController.pitch = 1.44f; // ドの音
            }
            else
            {
                // それ以外の場合、最初の弦の長さとの比に基づき音を割り当てる
                stringController.pitch = strPitch;
            }
        }
        // ピンの座標を保存
        markerPos = curPos;
        count += 1;
        Debug.Log("count: " + count.ToString());
        // 他人のピンと弦を張る
        DisplayStrWithOther(markerPos);
    }

    private void DisplayStrWithOther(Vector3 myMarkerPos)
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker"); // ピンを全取得
        Debug.Log("Acquired all markers.");
        foreach (GameObject marker in markers)
        {
            Debug.Log("Checking one marker.");
            PhotonView markerPhotonView = marker.GetComponent<PhotonView>();
            if (!markerPhotonView.IsMine)
            {
                Debug.Log("This marker is not mine.");
                Vector3 othersMarkerPos = marker.transform.position;
                float sqrDist = (othersMarkerPos - myMarkerPos).sqrMagnitude;
                float ratio = sqrDist / sqrInitLength;
                if (minRatio < ratio && ratio < maxRatio)
                {
                    foreach (float harmony in harmonies)
                    {
                        if (ratio < harmony)
                        {
                            // 弦をコライダーと共に設置
                            Vector3 strVec = othersMarkerPos - myMarkerPos; // 弦の方向を取得
                            float dist = strVec.magnitude;  // 弦の長さを取得
                            Vector3 strY = new Vector3(0f, dist, 0f);
                            Vector3 strVec2 = strVec * 0.5f;
                            Vector3 centerCoord = myMarkerPos + strVec2;    // 中点の座標        

                            GameObject str = Instantiate(othersStrPrefab, centerCoord, Quaternion.identity) as GameObject;
                            str.transform.localScale = new Vector3(strRadius, dist / 2, strRadius); // ひとまずY軸方向に伸ばす

                            CapsuleCollider col = str.GetComponent<CapsuleCollider>();
                            col.isTrigger = true;   // 衝突判定を行わないように
                            str.transform.rotation = Quaternion.FromToRotation(strY, strVec);   // 弦を回転

                            // 弦にプロパティ(pitch)を追加
                            StringController stringController = str.GetComponent<StringController>();
                            stringController.pitch = harmony;

                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnClickTrashButton()
    {
        Debug.Log("Clicked Trash Button.");
        expText.SetActive(false);
        pitchText.text = "";
        count = 0;
        sqrInitLength = 0;
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject marker in markers)
        {
            Destroy(marker);
        }
        Debug.Log("Deleted all my markers.");

        GameObject[] strs = GameObject.FindGameObjectsWithTag("String");
        foreach (GameObject str in strs)
        {
            Destroy(str);
        }
        Debug.Log("Deleted all strings.");        
    }
}
