using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MLAgents
{
    /// <summary>
    /// PropertyDrawer for BroadcastHub. Used to display the BroadcastHub in the Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(BroadcastHub))]
    public class BroadcastHubDrawer : PropertyDrawer
    {
        private BroadcastHub m_Hub;
        // The height of a line in the Unity Inspectors
        private const float k_LineHeight = 17f;
        // The vertical space left below the BroadcastHub UI.
        private const float k_ExtraSpaceBelow = 10f;

        /// <summary>
        /// Computes the height of the Drawer depending on the property it is showing
        /// </summary>
        /// <param name="property">The property that is being drawn.</param>
        /// <param name="label">The label of the property being drawn.</param>
        /// <returns>The vertical space needed to draw the property.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            LazyInitializeHub(property);
            var numLines = m_Hub.Count + 2 + (m_Hub.Count > 0 ? 1 : 0);
            return (numLines) * k_LineHeight + k_ExtraSpaceBelow;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LazyInitializeHub(property);
            position.height = k_LineHeight;
            EditorGUI.LabelField(position, new GUIContent(label.text,
                "The Broadcast Hub helps you define which Brains you want to expose to " +
                "the external process"));
            position.y += k_LineHeight;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.indentLevel++;
            DrawAddRemoveButtons(position);
            position.y += k_LineHeight;

            // This is the labels for each columns
            var brainWidth = position.width;
            var brainRect = new Rect(
                position.x, position.y, brainWidth, position.height);
            if (m_Hub.Count > 0)
            {
                EditorGUI.LabelField(brainRect, "Brains");
                brainRect.y += k_LineHeight;
            }
            DrawBrains(brainRect);
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws the Add and Remove buttons.
        /// </summary>
        /// <param name="position">The position at which to draw.</param>
        private void DrawAddRemoveButtons(Rect position)
        {
            // This is the rectangle for the Add button
            var addButtonRect = position;
            addButtonRect.x += 20;
            if (m_Hub.Count > 0)
            {
                addButtonRect.width /= 2;
                addButtonRect.width -= 24;
                var buttonContent = new GUIContent(
                    "Add New", "Add a new Brain to the Broadcast Hub");
                if (GUI.Button(addButtonRect, buttonContent, EditorStyles.miniButton))
                {
                    MarkSceneAsDirty();
                    AddBrain();
                }
                // This is the rectangle for the Remove button
                var removeButtonRect = position;
                removeButtonRect.x = position.width / 2 + 15;
                removeButtonRect.width = addButtonRect.width - 18;
                buttonContent = new GUIContent(
                    "Remove Last", "Remove the last Brain from the Broadcast Hub");
                if (GUI.Button(removeButtonRect, buttonContent, EditorStyles.miniButton))
                {
                    MarkSceneAsDirty();
                    RemoveLastBrain();
                }
            }
            else
            {
                addButtonRect.width -= 50;
                var buttonContent = new GUIContent(
                    "Add Brain to Broadcast Hub", "Add a new Brain to the Broadcast Hub");
                if (GUI.Button(addButtonRect, buttonContent, EditorStyles.miniButton))
                {
                    MarkSceneAsDirty();
                    AddBrain();
                }
            }
        }

        /// <summary>
        /// Draws the Brain  contained in the BroadcastHub.
        /// </summary>
        /// <param name="brainRect">The Rect to draw the Brains.</param>
        private void DrawBrains(Rect brainRect)
        {
            for (var index = 0; index < m_Hub.Count; index++)
            {
                var controlledBrains = m_Hub.brainsToControl;
                var brain = controlledBrains[index];
                // This is the rectangle for the brain
                EditorGUI.BeginChangeCheck();
                var newBrain = EditorGUI.ObjectField(
                    brainRect, brain, typeof(LearningBrain), true) as LearningBrain;
                brainRect.y += k_LineHeight;
                if (EditorGUI.EndChangeCheck())
                {
                    MarkSceneAsDirty();
                    m_Hub.brainsToControl.RemoveAt(index);
                    var brainToInsert = controlledBrains.Contains(newBrain) ? null : newBrain;
                    controlledBrains.Insert(index, brainToInsert);
                    break;
                }
            }
        }

        /// <summary>
        /// Lazy initializes the Drawer with the property to be drawn.
        /// </summary>
        /// <param name="property">The SerializedProperty of the BroadcastHub
        /// to make the custom GUI for.</param>
        private void LazyInitializeHub(SerializedProperty property)
        {
            if (m_Hub != null)
            {
                return;
            }
            var target = property.serializedObject.targetObject;
            m_Hub = fieldInfo.GetValue(target) as BroadcastHub;
            if (m_Hub == null)
            {
                m_Hub = new BroadcastHub();
                fieldInfo.SetValue(target, m_Hub);
            }
        }

        /// <summary>
        /// Signals that the property has been modified and requires the scene to be saved for
        /// the changes to persist. Only works when the Editor is not playing.
        /// </summary>
        private static void MarkSceneAsDirty()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// Removes the last Brain from the BroadcastHub
        /// </summary>
        private void RemoveLastBrain()
        {
            if (m_Hub.Count > 0)
            {
                m_Hub.brainsToControl.RemoveAt(m_Hub.brainsToControl.Count - 1);
            }
        }

        /// <summary>
        /// Adds a new Brain to the BroadcastHub. The value of this brain will not be initialized.
        /// </summary>
        private void AddBrain()
        {
            m_Hub.brainsToControl.Add(null);
        }
    }
}
