﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;

namespace dnGREP.WPF
{
    /// <summary>
    /// Interaction logic for ScriptEditorWindow.xaml
    /// </summary>
    public partial class ScriptEditorWindow : ThemedWindow
    {
        private readonly ScriptViewModel viewModel;

        public ScriptEditorWindow()
        {
            InitializeComponent();

            viewModel = new ScriptViewModel(textEditor);
            DataContext = viewModel;

            viewModel.RequestClose += (s, e) => Close();

            SearchPanel.Install(textEditor);

            textEditor.ShowLineNumbers = true;

            textEditor.TextArea.TextEntering += TextArea_TextEntering;
            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            textEditor.TextArea.KeyDown += TextArea_KeyDown;
            textEditor.TextArea.KeyUp += TextArea_KeyUp;
        }

        CompletionWindow completionWindow;
        void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Suggest();
                e.Handled = true;
            }

        }

        private void TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                Suggest();
                e.Handled = true;
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == " " || e.Text == "\n")
            {
                var caret = textEditor.TextArea.Caret;
                string lineText = GetLineText(caret);

                if (lineText.StartsWith("//"))
                {
                    return;
                }

                if (e.Text == "\n" && !string.IsNullOrEmpty(lineText))
                {
                    return;
                }

                Suggest();
            }
        }

        private void Suggest()
        {
            var caret = textEditor.TextArea.Caret;
            string lineText = GetLineText(caret);
            int wordIndex = GetWordIndex(lineText, caret);
            var stmt = ScriptManager.Instance.ParseLine(lineText, caret.Line);

            if (stmt != null && wordIndex == 0)
            {
                wordIndex++;
            }

            List<ScriptingCompletionData> scriptingData = null;
            string valueHint = null;

            if (wordIndex == 0)
            {
                scriptingData = ScriptManager.CommandCompletionData;
            }
            else if (stmt != null && wordIndex == 1)
            {
                var cmd = ScriptManager.ScriptCommands.FirstOrDefault(c => c.Command == stmt.Command);
                if (cmd != null)
                {
                    if (cmd.CompletionData.Count > 0)
                    {
                        scriptingData = cmd.CompletionData;
                    }
                    else if (!string.IsNullOrEmpty(cmd.ValueHintKey))
                    {
                        valueHint = cmd.ValueHint;
                    }
                }
            }
            else if (stmt != null && !string.IsNullOrEmpty(stmt.Target))
            {
                var cmd = ScriptManager.ScriptCommands.FirstOrDefault(c => c.Command == stmt.Command);
                if (cmd != null)
                {
                    var target = cmd.Targets.FirstOrDefault(t => t.Target == stmt.Target);
                    if (target != null)
                    {
                        if (target.CompletionData.Count > 0)
                        {
                            scriptingData = target.CompletionData;
                        }
                        else if (!string.IsNullOrEmpty(target.ValueHintKey))
                        {
                            valueHint = target.ValueHint;
                        }
                    }
                }
            }

            if (scriptingData != null && scriptingData.Count > 0)
            {
                completionWindow = new CompletionWindow(textEditor.TextArea)
                {
                    FontSize = viewModel.ResultsFontSize,
                    Width = double.NaN,
                    SizeToContent = System.Windows.SizeToContent.WidthAndHeight
                };
                AddRange(scriptingData, completionWindow.CompletionList.CompletionData);
                completionWindow.Closed += (s, e) =>
                {
                    completionWindow = null;
                };
                completionWindow.Show();
            }
            else if (!string.IsNullOrEmpty(valueHint))
            {
                var insightWindow = new InsightWindow(textEditor.TextArea)
                {
                    Content = valueHint,
                    //Background = System.Windows.Media.Brushes.Linen
                };
                insightWindow.Closed += (s, e) =>
                {
                    completionWindow = null;
                };
                insightWindow.Show();
            }
        }

        private void AddRange(IList<ScriptingCompletionData> source, IList<ICompletionData> destination)
        {
            foreach (var item in source)
            {
                destination.Add(item);
            }
        }

        private string GetLineText(Caret caret)
        {
            var docLine = textEditor.Document.GetLineByNumber(caret.Line);
            return textEditor.Document.GetText(docLine.Offset, docLine.Length);
        }

        private int GetWordIndex(string lineText, Caret caret)
        {
            return lineText.Substring(0, caret.VisualColumn).TrimStart().Count(c => c == ' ');
        }

        private string GetWordAtCaret(string lineText, Caret caret)
        {
            VisualLine visualLine = textEditor.TextArea.TextView.GetVisualLine(caret.Line);
            int offsetStart = visualLine.GetNextCaretPosition(caret.VisualColumn, LogicalDirection.Backward, CaretPositioningMode.WordBorder, true);
            int offsetEnd = visualLine.GetNextCaretPosition(caret.VisualColumn, LogicalDirection.Forward, CaretPositioningMode.WordBorder, true);

            if (offsetEnd == -1 || offsetStart == -1)
                return string.Empty;

            var currentChar = lineText.Substring(caret.VisualColumn, 1);

            if (string.IsNullOrWhiteSpace(currentChar))
                return string.Empty;

            return lineText.Substring(offsetStart, offsetEnd - offsetStart);
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // do not set e.Handled=true - we still want to insert the character that was typed
        }

        public string ScriptFile
        {
            get => viewModel.ScriptFile;
            set => viewModel.ScriptFile = value;
        }
    }
}
