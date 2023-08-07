using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UbiqReadyPlayerMe.Samples
{
    // Quick sample script to show loading status and visualize avatars
    public class UbiqReadyPlayerMeExample : MonoBehaviour
    {
        private UbiqReadyPlayerMeLoader loader;
        private Text argsText;

        private void Start()
        {
            transform.Find("Edit Mode").gameObject.SetActive(false);
            transform.Find("Play Mode").gameObject.SetActive(true);

            argsText = transform.Find("Play Mode/Status Args Text").GetComponent<Text>();
            argsText.text = "Waiting for avatar to load...";
        }

        private void Update()
        {
            if (!loader)
            {
                loader = FindObjectOfType<UbiqReadyPlayerMeLoader>();
                if (loader)
                {
                    loader.completed.AddListener(Loader_Completed);
                    loader.failed.AddListener(Loader_Failed);
                }
            }
        }

        private void OnDestroy()
        {
            if (loader)
            {
                loader.completed.RemoveListener(Loader_Completed);
                loader.failed.RemoveListener(Loader_Failed);
            }
        }

        private void Loader_Completed(ReadyPlayerMe.AvatarLoader.CompletionEventArgs args)
        {
            argsText.text = args.Url;
        }

        private void Loader_Failed(ReadyPlayerMe.AvatarLoader.FailureEventArgs args)
        {
            argsText.text = args.Message;
        }
    }
}
