using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableAsset : MonoBehaviour
{
    [SerializeField] TMP_Text sizeText;
    [SerializeField] Slider sizeSlider;
    [SerializeField] RectTransform addressableUI;
    [SerializeField] RectTransform loadingSceneUI;

    [Space]
    [Header("다운로드를 원하는 번들 또는 번들들에 포함된 레이블중 아무거나 입력해주세요.")]
    [SerializeField] string LableForBundleDown = string.Empty;

    private void Start()
    {
        //OnClick_BundleDelete();
        Click_CheckTheDownloadFileSize();
    }

    #region 에셋번들 다운로드
    /// <summary> 에셋번들 다운로드 </summary>
    public void Click_BundleDown()
    {
        StartCoroutine(BundleDownCo());
    }

    IEnumerator BundleDownCo()
    {
        var handle = Addressables.DownloadDependenciesAsync(LableForBundleDown, false);
        while (!handle.IsDone)
        {
            sizeSlider.value = handle.PercentComplete;
            yield return null;
        }

        //다운로드 완료
        Debug.Log("DownloadComplete!");
        Addressables.Release(handle);
        addressableUI.gameObject.SetActive(false);
        loadingSceneUI.gameObject.SetActive(true);//로딩씬UI
    }
    #endregion

    /// <summary> 에셋번들 다운로드 사이즈 체크 </summary>
    public void Click_CheckTheDownloadFileSize()
    {
        //크기를 확인할 번들 또는 번들들에 포함된 레이블을 인자로 주면 됨.
        //long타입으로 반환되는게 특징
        Addressables.GetDownloadSizeAsync(LableForBundleDown).Completed +=
            (AsyncOperationHandle<long> SizeHandle) =>
            {
                //이미 다운로드 되어있으면 리턴
                if (SizeHandle.Result <= 0)
                {
                    loadingSceneUI.gameObject.SetActive(true);//로딩씬UI
                    return;
                }

                string size = "";
                if (SizeHandle.Result >= 1073741824.0)
                {
                    size = string.Format("{0:##.##}", SizeHandle.Result / 1073741824.0) + " GB";
                }
                else if (SizeHandle.Result >= 1048576.0)
                {
                    size = string.Format("{0:##.##}", SizeHandle.Result / 1048576.0) + " MB";
                }
                else if (SizeHandle.Result >= 1024.0)
                {
                    size = string.Format("{0:##.##}", SizeHandle.Result / 1024.0) + " KB";
                }
                else if (SizeHandle.Result > 0)
                {
                    size = string.Concat(SizeHandle.Result, " byte");
                }
                sizeText.text = size;

                addressableUI.gameObject.SetActive(true);

                //메모리 해제
                Addressables.Release(SizeHandle);
            };


    }

    /// <summary> 번들 데이터 삭제 </summary>
    public void OnClick_BundleDelete()
    {
        Addressables.ClearDependencyCacheAsync(LableForBundleDown);
    }


}
