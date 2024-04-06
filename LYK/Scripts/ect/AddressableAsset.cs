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
    [Header("�ٿ�ε带 ���ϴ� ���� �Ǵ� ����鿡 ���Ե� ���̺��� �ƹ��ų� �Է����ּ���.")]
    [SerializeField] string LableForBundleDown = string.Empty;

    private void Start()
    {
        //OnClick_BundleDelete();
        Click_CheckTheDownloadFileSize();
    }

    #region ���¹��� �ٿ�ε�
    /// <summary> ���¹��� �ٿ�ε� </summary>
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

        //�ٿ�ε� �Ϸ�
        Debug.Log("DownloadComplete!");
        Addressables.Release(handle);
        addressableUI.gameObject.SetActive(false);
        loadingSceneUI.gameObject.SetActive(true);//�ε���UI
    }
    #endregion

    /// <summary> ���¹��� �ٿ�ε� ������ üũ </summary>
    public void Click_CheckTheDownloadFileSize()
    {
        //ũ�⸦ Ȯ���� ���� �Ǵ� ����鿡 ���Ե� ���̺��� ���ڷ� �ָ� ��.
        //longŸ������ ��ȯ�Ǵ°� Ư¡
        Addressables.GetDownloadSizeAsync(LableForBundleDown).Completed +=
            (AsyncOperationHandle<long> SizeHandle) =>
            {
                //�̹� �ٿ�ε� �Ǿ������� ����
                if (SizeHandle.Result <= 0)
                {
                    loadingSceneUI.gameObject.SetActive(true);//�ε���UI
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

                //�޸� ����
                Addressables.Release(SizeHandle);
            };


    }

    /// <summary> ���� ������ ���� </summary>
    public void OnClick_BundleDelete()
    {
        Addressables.ClearDependencyCacheAsync(LableForBundleDown);
    }


}
