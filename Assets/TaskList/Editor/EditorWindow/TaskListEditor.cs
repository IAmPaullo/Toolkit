using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;


namespace Paullo
{
    public class TaskListEditor : EditorWindow
    {
        TaskListSO taskListSO;
        VisualElement container;

        ObjectField savedTasksObjectField;
        ProgressBar taskProgressBar;

        TextField taskText;
        ScrollView taskListScrollView;
        ToolbarSearchField searchBox;



        Button addTaskButton;
        Button loadTasksButton;
        Button saveProgressButton;

        public const string path = "Assets/TaskList/Editor/EditorWindow/";

        [MenuItem("Tools/TaskList")]
        public static void ShowWindow()
        {
            TaskListEditor windows = GetWindow<TaskListEditor>();
            windows.titleContent = new GUIContent("Task List");
        }

        public void CreateGUI()
        {
            container = rootVisualElement;
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskListEditor.uxml");
            container.Add(original.Instantiate());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            container.styleSheets.Add(styleSheet);


            savedTasksObjectField = container.Q<ObjectField>("savedTasksObjectField");
            savedTasksObjectField.objectType = typeof(TaskListSO);

            loadTasksButton = container.Q<Button>("loadTasksButton");
            loadTasksButton.clicked += LoadTasks;
            saveProgressButton = container.Q<Button>("saveProgressButton");
            saveProgressButton.clicked += SaveProgress;

            searchBox = container.Q<ToolbarSearchField>("searchBox");
            searchBox.RegisterValueChangedCallback(OnSearchTextChange);


            taskText = container.Q<TextField>("taskText");
            taskText.RegisterCallback<KeyDownEvent>(AddTask);

            addTaskButton = container.Q<Button>("addTaskButton");
            addTaskButton.clicked += AddTask;


            taskListScrollView = container.Q<ScrollView>("taskListScrollView");
            taskProgressBar = container.Q<ProgressBar>("taskProgressBar");
        }




        private void AddTask()
        {
            if (!string.IsNullOrEmpty(taskText.value))
            {

                taskListScrollView.Add(CreateTask(taskText.value));
                SaveTask(taskText.value);
                taskText.value = "";
                taskText.Focus();
                UpdateProgress();
            }

        }

        private TaskItem CreateTask(string taskText)
        {
            TaskItem taskItem = new TaskItem(taskText);
            taskItem.GetTaskLabel().text = taskText;
            taskItem.GetTaskToggle().RegisterValueChangedCallback(UpdateProgress);
            return taskItem;
        }



        void AddTask(KeyDownEvent evt)
        {
            if (Event.current.Equals(Event.KeyboardEvent("Return")))
            {
                AddTask();
            }
        }


        void LoadTasks()
        {
            taskListSO = savedTasksObjectField.value as TaskListSO;

            if (taskListSO != null)
            {
                taskListScrollView.Clear();
                List<string> tasks = taskListSO.GetTasks();

                foreach (string task in tasks)
                {
                    taskListScrollView.Add(CreateTask(task));
                }
                UpdateProgress();
            }
        }

        void SaveTask(string task)
        {
            taskListSO.AddTask(task);
            EditorUtility.SetDirty(taskListSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void SaveProgress()
        {
            if (taskListSO != null)
            {
                List<string> tasks = new List<string>();

                foreach (TaskItem task in taskListScrollView.Children())
                {
                    if (!task.GetTaskToggle().value)
                    {
                        tasks.Add(task.GetTaskLabel().text);
                    }
                }
                taskListSO.AddTasks(tasks);
                EditorUtility.SetDirty(taskListSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LoadTasks();
            }
        }


        void UpdateProgress()
        {
            int count = 0;
            int completedTasks = 0;


            foreach (TaskItem task in taskListScrollView.Children())
            {
                if (task.GetTaskToggle().value)
                {
                    completedTasks++;
                }
                count++;
            }
            if (count > 0)
            {
                float progress = completedTasks / (float)count;
                taskProgressBar.value = progress;
                taskProgressBar.title = string.Format("{0} %", Mathf.Round(progress * 100));


            }
            else
            {
                taskProgressBar.value = 1;
                taskProgressBar.title = string.Format("{0} %", 100);
            }
        }

        void UpdateProgress(ChangeEvent<bool> e)
        {
            UpdateProgress();
        }

        void OnSearchTextChange(ChangeEvent<string> changeEvent)
        {
            string searchText = changeEvent.newValue.ToLower();

            foreach (TaskItem task in taskListScrollView.Children())
            {
                string taskText = task.GetTaskLabel().text.ToLower();

                if (!string.IsNullOrEmpty(searchText) && taskText.Contains(searchText))
                {
                    task.GetTaskLabel().AddToClassList("highlight");
                }
                else
                {
                    task.GetTaskLabel().RemoveFromClassList("highlight");

                }
            }
        }
    }
}