using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace redwyre.DevTools.Editor.Terminal
{
    [UxmlElement("ConsoleText")]
    public partial class ConsoleTextElement : TextElement
    {
        float characterWidth = 1.0f;
        float characterHeight = 1.0f;

        float cursorWidth = 1.0f;
        float cursorHeight = 1.0f;

        Color32 cursorColour = Color.gray;

        public ConsoleTextElement()
            : base()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<GeometryChangedEvent>(GeometryChangedEvent);
        }

        private static void GeometryChangedEvent(GeometryChangedEvent evt)
        {
            if (evt.target is ConsoleTextElement ve)
            {
                var size = ve.MeasureTextSize("M", 0.0f, MeasureMode.Undefined, 0.0f, MeasureMode.Undefined);
                ve.characterWidth = AlignmentUtils.CeilToPanelPixelSize(ve, size.x);
                ve.characterHeight = AlignmentUtils.CeilToPanelPixelSize(ve, size.y);

                ve.cursorHeight = ve.characterHeight;
            }
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            DrawCaret(context);
        }

        void DrawCaret(MeshGenerationContext mgc)
        {
            var colour = cursorColour;
            var pos = new Vector3(selection.cursorPosition.x, selection.cursorPosition.y - cursorHeight, Vertex.nearZ);
            var size = new Vector2(cursorWidth, cursorHeight);

            mgc.AllocateTempMesh(4, 6, out var verts, out var indices);

            verts[0] = new Vertex { tint = colour, position = pos + new Vector3(0.0f, 0.0f) };
            verts[1] = new Vertex { tint = colour, position = pos + new Vector3(size.x, 0.0f) };
            verts[2] = new Vertex { tint = colour, position = pos + new Vector3(size.x, size.y) };
            verts[3] = new Vertex { tint = colour, position = pos + new Vector3(0.0f, size.y) };

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;

            mgc.DrawMesh(verts, indices);
        }
    }
}