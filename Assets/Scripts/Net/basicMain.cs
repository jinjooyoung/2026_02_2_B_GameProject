using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class basicMain : MonoBehaviour
{
    public Button Hello;
    public string host;             // IP 주소 (로커에서 127.0.0.1)
    public string port;             // 포트 주소 (3000)
    public string route;            // 라우트 주소

    private void Start()
    {
        this.Hello.onClick.AddListener(() =>
        {
            var url = string.Format("{0}:{1}/{2}", host, port, route);
            Debug.Log(url);

            StartCoroutine(this.GetBasic(url, (raw) =>
            {
                Debug.LogFormat("{0}", raw);
            }));
        });
    }

    private IEnumerator GetBasic(string url, System.Action<string> callback)
    {
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("네트워크 통신 에러");
        }
        else
        {
            callback(webRequest.downloadHandler.text);      // 통신 완료되고 해당 텍스트를 가져온다.
        }
    }
}
