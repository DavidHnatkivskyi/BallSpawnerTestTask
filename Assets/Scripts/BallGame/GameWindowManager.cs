using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BallGame
{
    internal sealed class GameWindowManager
    {
        private GameObject _winWindow;
        private GameObject _loseWindow;
        private Action _restartAction;

        public void Initialize(Action restartAction)
        {
            _restartAction = restartAction;

            var canvasObject = new GameObject("Game Windows");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();
            _winWindow = CreateWindow(canvasObject.transform, "Win Window", "Ви виграли", new Color(0.2f, 0.75f, 0.35f));
            _loseWindow = CreateWindow(canvasObject.transform, "Lose Window", "Ви програли", new Color(0.85f, 0.2f, 0.25f));
            HideAll();
        }

        public void ShowWin()
        {
            Show(_winWindow);
        }

        public void ShowLose()
        {
            Show(_loseWindow);
        }

        public void HideAll()
        {
            _winWindow.SetActive(false);
            _loseWindow.SetActive(false);
        }

        private void Show(GameObject window)
        {
            HideAll();
            window.SetActive(true);
        }

        private GameObject CreateWindow(Transform parent, string name, string title, Color titleColor)
        {
            var window = new GameObject(name, typeof(RectTransform), typeof(Image));
            window.transform.SetParent(parent, false);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(620f, 320f);
            window.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.13f, 0.95f);

            var titleText = CreateText(window.transform, title, 52, new Vector2(0f, 55f));
            titleText.color = titleColor;
            CreateRestartButton(window.transform);
            return window;
        }

        private static Text CreateText(Transform parent, string content, int fontSize, Vector2 position)
        {
            var textObject = new GameObject("Title", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(560f, 90f);
            rect.anchoredPosition = position;

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = content;
            return text;
        }

        private void CreateRestartButton(Transform parent)
        {
            var buttonObject = new GameObject("Restart Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(260f, 74f);
            rect.anchoredPosition = new Vector2(0f, -75f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.55f, 0.95f, 1f);
            buttonObject.GetComponent<Button>().onClick.AddListener(Restart);

            var label = CreateText(buttonObject.transform, "Restart", 30, Vector2.zero);
            label.color = Color.white;
        }

        private void Restart()
        {
            HideAll();
            _restartAction.Invoke();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UnityEngine.Object.DontDestroyOnLoad(eventSystemObject);
        }
    }
}
