﻿using InGameCodeEditor;
using InGameCodeEditor.Lexer;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeInspector
{
    /// <summary>
    /// The main InGame Code Editor component for displaying a syntax highlighting code editor UI element.
    /// </summary>
    public class CustomCodeEditor : MonoBehaviour
    {


        // Private 
        private static readonly KeyCode[] focusKeys = { KeyCode.Return, KeyCode.Backspace, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
        private static StringBuilder highlightedBuilder = new StringBuilder(4096);
        private static StringBuilder lineBuilder = new StringBuilder();
        private static MethodInfo scrollBarUpdateFix = null;

        private InputStringLexer lexer = new InputStringLexer();
        private RectTransform inputTextTransform = null;
        private RectTransform lineHighlightTransform = null;
        private int lineCount = 0;
        private int currentLine = 0;
        private int currentColumn = 0;
        private int currentIndent = 0;
        private string lastText = null;
        private bool delayedRefresh = false;
        private float lastScrollValue = 0f;
        private bool lineHighlightLocked = false;

        // Complains about references never assigned but they are inspector values
#pragma warning disable 0649
        [Header("Elements")]
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private TextMeshProUGUI inputText;
        [SerializeField]
        private TextMeshProUGUI inputHighlightText;
        [SerializeField]
        private TextMeshProUGUI lineText;
        [SerializeField]
        private Image background;
        [SerializeField]
        private Image lineHighlight;
        [SerializeField]
        private int _perLineHightlightOffset = 4;
        [SerializeField]
        private Image lineNumberBackground;
        [SerializeField]
        private Image scrollbar;
        [SerializeField]
        private Scrollbar _verticalScrollbar;

        [Header("Text Sections")]
        [SerializeField]
        private TextMeshProUGUI beforeInputText;
        [SerializeField]
        private TextMeshProUGUI beforeInputHighlightText;
        [SerializeField]
        private TextMeshProUGUI afterInputText;
        [SerializeField]
        private TextMeshProUGUI afterInputHighlightText;


        [Header("Themes")]
        [SerializeField]
        private CodeEditorTheme editorTheme = null;
        [SerializeField]
        private CodeLanguageTheme languageTheme = null;

        [Header("Options")]
        [SerializeField]
        private bool lineNumbers = true;
        [SerializeField]
        private int lineNumbersSize = 20;

#if UNITY_2018_2_OR_NEWER
        [Header("TMP Compatibility")]
        [SerializeField]
        private bool applyLineOffsetFix = false;
#endif
#pragma warning restore 0649


        // Properties
        /// <summary>
        /// The current editor theme that is being used by the code editor.
        /// This value will be null if no theme is assigned but the code editor will revert to built in default colors.
        /// </summary>
        public CodeEditorTheme EditorTheme
        {
            get { return editorTheme; }
            set
            {
                editorTheme = value;
                ApplyTheme();
            }
        }

        /// <summary>
        /// The current language theme that is being used by the code editor.
        /// The language theme controls which aspects of the _text are syntax highlighted.
        /// You can set this value to null to disable syntax highlighting.
        /// </summary>
        public CodeLanguageTheme LanguageTheme
        {
            get { return languageTheme; }
            set
            {
                languageTheme = value;
                ApplyLanguage();
            }
        }

        /// <summary>
        /// Get the TextMesh Pro input field that this code editor is managing.
        /// </summary>
        public TMP_InputField InputField
        {
            get { return inputField; }
        }

        /// <summary>
        /// Get the total number of lines that the _text occupies.
        /// </summary>
        public int LineCount
        {
            get { return lineCount; }
        }

        /// <summary>
        /// Get the current line number for the caret position.
        /// </summary>
        public int CurrentLine
        {
            get { return currentLine; }
        }

        /// <summary>
        /// Get the current column number for the caret position.
        /// </summary>
        public int CurrentColumn
        {
            get { return currentColumn; }
        }

        /// <summary>
        /// Get the current indent level for the caret position.
        /// </summary>
        public int CurrentIndent
        {
            get { return currentIndent; }
        }

        /// <summary>
        /// The _text of the code editor input field.
        /// Assigning _text will automatically cause a refresh so you do not need to call it manually.
        /// </summary>
        public string Text
        {
            get { return beforeText + '\n' + inputField.text + '\n' + afterText; }
        }


        private string beforeText = "";


        private string afterText = "";

        // Properties to manage the three sections
        public string BeforeText
        {
            get { return beforeText; }
            set
            {
                beforeText = value ?? "";
                beforeInputText.text = beforeText;
                beforeInputHighlightText.text = SyntaxHighlightContent(beforeText);
                UpdateLayout();
            }
        }

        public string AfterText
        {
            get { return afterText; }
            set
            {
                afterText = value ?? "";
                afterInputText.text = afterText;
                afterInputHighlightText.text = SyntaxHighlightContent(afterText);
                UpdateLayout();
            }
        }

        /// <summary>
        /// Get the current _text including xml color tags generated by the syntax highlighter.
        /// </summary>
        public string HighlightedText
        {
            get { return inputHighlightText.text; }
        }

        /// <summary>
        /// Is the line numbers column enabled.
        /// Setting this value to false will cause the column to be hidden.
        /// </summary>
        public bool LineNumbers
        {
            get { return lineNumbers; }
            set
            {
                lineNumbers = value;

                RectTransform inputFieldTransform = inputField.transform as RectTransform;
                RectTransform lineNumberBackgroudTransform = lineNumberBackground.transform as RectTransform;

                // Check for line numbers
                if (lineNumbers == true)
                {
                    // EnableMiniGame line numbers
                    lineNumberBackground.gameObject.SetActive(true);
                    lineText.gameObject.SetActive(true);

                    // Set left value
                    //inputFieldTransform.offsetMin = new Vector2(lineNumbersSize, inputFieldTransform.offsetMin.y);
                    //lineNumberBackgroudTransform.sizeDelta = new Vector2(lineNumbersSize + 15, lineNumberBackgroudTransform.sizeDelta.y);
                }
                else
                {
                    // DisableMiniGame line numbers
                    lineNumberBackground.gameObject.SetActive(false);
                    lineText.gameObject.SetActive(false);

                    //// Set left value
                    //inputFieldTransform.offsetMin = new Vector2(0, inputFieldTransform.offsetMin.y);
                }
            }
        }

        /// <summary>
        /// The current size of the line number column.
        /// Default size is 20.
        /// </summary>
        public int LineNumbersSize
        {
            get { return lineNumbersSize; }
            set
            {
                lineNumbersSize = value;

                // Update the line numbers
                LineNumbers = lineNumbers;
            }
        }

        // Methods
#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void OnValidate()
        {
            // Update line numbers
            LineNumbersSize = lineNumbersSize;

            // Appy the theme
            if (AllReferencesAssigned() == true)
                if (editorTheme != null)
                    ApplyTheme();

            // Rebuild language colors
            if (languageTheme != null)
                languageTheme.Invalidate();
        }
#endif

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Awake()
        {
            // Check for invalid references
            if (AllReferencesAssigned() == false)
            {
                enabled = false;
                throw new MissingReferenceException("One or more required references are missing. Make sure all references under the 'Elements' header are assigned");
            }

            // Cache transform
            this.inputTextTransform = inputText.GetComponent<RectTransform>();
            this.lineHighlightTransform = lineHighlight.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Load default theme
            if (editorTheme == null)
                editorTheme = CodeEditorTheme.DefaultTheme;

            // Apply the theme
            ApplyTheme();
            ApplyLanguage();

            inputField.onValueChanged.AddListener(ValidateBrackets);
        }
        private void ValidateBrackets(string newText)
        {
            if (_disableClosingBracketPrevention)
                return;

            int openCount = 0;
            int closeCount = 0;

            foreach (char c in newText)
            {
                if (c == '{') openCount++;
                if (c == '}') closeCount++;
            }

            // Prevent input if closing brackets exceed opening brackets
            if (closeCount > openCount)
            {
                // Revert to previous valid text state
                inputField.text = RemoveExcessClosingBrackets(newText);
                inputField.caretPosition = inputField.text.Length; // Maintain cursor position
            }
        }

        private string RemoveExcessClosingBrackets(string text)
        {
            int openCount = 0, closeCount = 0;
            char[] characters = text.ToCharArray();

            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == '{') openCount++;
                if (characters[i] == '}')
                {
                    closeCount++;
                    if (closeCount > openCount)
                    {
                        characters[i] = '\0'; // Mark for removal
                        closeCount--; // Revert the invalid count
                    }
                }
            }

            return new string(characters).Replace("\0", ""); // Remove invalid characters
        }
        public void SetText(string before, string input, string after)
        {
            BeforeText = before;

            inputField.caretPosition = 0;
            inputField.text = string.IsNullOrEmpty(input) ? "\n" : input;
            AfterText = after;
            delayedRefresh = true;
            UpdateLayout();
        }


        private void UpdateLayout()
        {
            // We no longer need to manually position elements since the Vertical Layout Group handles that
            // Just make sure everything is properly marked for rebuild
            LayoutRebuilder.MarkLayoutForRebuild(beforeInputText.rectTransform);
            LayoutRebuilder.MarkLayoutForRebuild(inputText.rectTransform);
            LayoutRebuilder.MarkLayoutForRebuild(afterInputText.rectTransform);

            // Update line numbers and highlighting
            UpdateCurrentLineNumbers();
            UpdateCurrentLineHighlight();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void LateUpdate()
        {
            if (Input.mouseScrollDelta != Vector2.zero || _verticalScrollbar.value != lastScrollValue)
            {
                UpdateCurrentLineHighlight();
                lastScrollValue = _verticalScrollbar.value;
            }

            // Auto indent
            if (languageTheme != null && languageTheme.autoIndent.autoIndentMode != AutoIndent.IndentMode.None)// languageTheme.autoIndent.allowAutoIndent == true)
            {
                // Check for new line
                if (Input.GetKeyDown(KeyCode.Return) == true)
                {
                    // Auto indent for new line
                    AutoIndentCaret(false);
                }
                else if (Input.anyKeyDown == true && Input.inputString.Contains(languageTheme.autoIndent.IndentDecreaseString) == true)
                {
                    // Auto indent for closing token
                    AutoIndentCaret(true);
                }
            }

            // Make sure the input field is focused
            if (inputField.isFocused == true || delayedRefresh == true)
            {
                // Check for delayed refresh caused by pasting _text
                if (delayedRefresh == true)
                {
                    delayedRefresh = false;
                    Refresh(true, false);
                }

                // Check for paste _text
                if (Input.GetKey(KeyCode.LeftControl) == true && Input.GetKeyDown(KeyCode.V) == true)
                {
                    // Refresh full _text on the next frame after tmp has updated its _text infos
                    delayedRefresh = true;
                }

                // Check if we are typing
                if (Input.anyKey == true)
                    Refresh();

                bool focusKeyPressed = false;

                // Check for any focus key pressed
                foreach (KeyCode key in focusKeys)
                {
                    if (Input.GetKey(key) == true)
                    {
                        focusKeyPressed = true;
                        break;
                    }
                }

                // Update line highlight
                if (focusKeyPressed == true || Input.GetMouseButton(0))
                    UpdateCurrentLineHighlight();
            }
        }

        /// <summary>
        /// Causes the displayed _text content to be refreshed and rehighlighted if it has changed.
        /// </summary>
        /// <param name="forceUpdate">Forcing an update will cause the _text to be refreshed even if it has not changed</param>
        /// <param name="updateLineOnly">Should only the current line be refreshed or the whole _text</param>
        public void Refresh(bool forceUpdate = false, bool updateLineOnly = true)
        {
            // Trigger a content change event
            DisplayedContentChanged(inputField.text, forceUpdate, updateLineOnly);
        }

        /// <summary>
        /// Set the line where the line highlight bar will be positioned. Valid line numbers start at 1 and count up until <see cref="LineCount"/>.
        /// You may also like to lock the line highlight bar in position to prevent it being moved by the user which can be achieved by passing 'true' as second argument.
        /// </summary>
        /// <param name="lineNumber">The absolute line number to move the line highlight bar to</param>
        /// <param name="lockLineHighlight">True if the line highlight bar should be locked after moving to the specified line or false if the line bar should be unlocked.</param>
        public void SetLineHighlight(int lineNumber, bool lockLineHighlight)
        {
            // Check if code editor is not active
            if (isActiveAndEnabled == false || lineNumber < 1 || lineNumber > LineCount)
                return;

            int lineOffset = 0;
            int lineIndex = lineNumber - 1;// inputText.textInfo.lineCount - lineNumber - 1;

#if UNITY_2018_2_OR_NEWER
            if (applyLineOffsetFix == true)
                lineOffset++;
#endif

            // Highlight the current line
            lineHighlightTransform.anchoredPosition = new Vector2(5,
                (inputText.textInfo.lineInfo[inputText.textInfo.characterInfo[0].lineNumber].lineHeight *
                -lineIndex) + lineOffset - 4f +
                inputTextTransform.anchoredPosition.y);

            // Lock the line highlight so it cannot be moved
            if (lockLineHighlight == true)
                LockLineHighlight();
            else
                UnlockLineHighlight();
        }

        /// <summary>
        /// Lock the line highlight bar at the current line. Mouse or keyboard events will not affect the position of the line highlight bar until <see cref="UnlockLineHighlight"/> is called.
        /// </summary>
        public void LockLineHighlight()
        {
            lineHighlightLocked = true;
        }

        /// <summary>
        /// Unlock the line highlight bar. Mouse or keyboard events will cause the line highlight bar to be updated as the user moves to different lines.
        /// </summary>
        public void UnlockLineHighlight()
        {
            lineHighlightLocked = false;
        }

        private void DisplayedContentChanged(string newText, bool forceUpdate, bool updateLineOnly)
        {
            // Update caret position
            UpdateCurrentLineColumnIndent();
            // Check for change
            if ((forceUpdate == false && lastText == newText) || string.IsNullOrEmpty(newText) == true)
            {
                if (string.IsNullOrEmpty(newText) == true)
                {
                    inputHighlightText.text = string.Empty;
                }

                // Its possible the _text was cleared so we need to sync numbers and highlighter
                UpdateCurrentLineNumbers();
                UpdateCurrentLineHighlight();
                return;
            }

            //if (updateLineOnly == false)
            //{
            // Run parser to highlight keywords
            inputHighlightText.text = SyntaxHighlightContent(newText);
            //}
            //else
            //{
            //    // Get the caret position
            //    int editIndex = inputField.stringPosition;

            //    // Get the current line
            //    TMP_LineInfo line = inputText.textInfo.lineInfo[currentLine];

            //    int start = line.firstCharacterIndex;
            //    int length = line.characterCount;

            //    // Get the substring
            //    string workingString = newText.Substring(start, length);

            //    // Run the parser on the line
            //    string highlightedWorkingString = SyntaxHighlightContent(workingString);

            //    // Insert the highlighted _text
            //    inputHighlightText._text = inputHighlightText._text.Remove(start, length - 1);
            //    inputHighlightText._text = inputHighlightText._text.Insert(start, highlightedWorkingString);
            //}

            // Autohide scrollbar
            bool showScrollbar = _verticalScrollbar.size <= 1f;

            // Show the scrollbar
            _verticalScrollbar.gameObject.SetActive(showScrollbar);


            // Sync line numbers and update the line highlight
            UpdateCurrentLineNumbers();
            UpdateCurrentLineHighlight();

            this.lastText = newText;
        }


        private void Update()
        {
            if (inputField != null)
            {
                inputField.interactable = CodeVanguardManager.Instance.PlayerModeToggler.CodeModeActive;
            }
        }
        //MTZ-Refactor
        private void UpdateCurrentLineNumbers()
        {
            // Count total lines across all sections
            string fullText = Text;
            int currentLineCount = fullText.Split('\n').Length;
            lineBuilder.Clear();
            int currentLineNumber = 1;

            // Handle empty text case
            if (string.IsNullOrEmpty(fullText))
            {
                lineText.text = "1";
                lineCount = 1;
                return;
            }

            for (int i = 0; i < fullText.Length; i++)
            {
                // Add line number at start of text or after newline
                if (i == 0 || fullText[i - 1] == '\n')
                {
                    lineBuilder.Append(currentLineNumber);
                    lineBuilder.Append('\n');
                    currentLineNumber++;
                }
            }

            // Always add the last line number if we haven't already

            //lineBuilder.Append(currentLineNumber);


            lineText.text = lineBuilder.ToString();
            lineCount = currentLineCount;
        }
        //Old method in case the other is actually broken

        //private void UpdateCurrentLineNumbers()
        //{
        //    // Get the line count
        //    int currentLineCount = inputText.textInfo.lineCount;

        //    int currentLineNumber = 1;

        //    // Check for a change in line
        //    if (currentLineCount != lineCount)
        //    {
        //        // Update line numbers
        //        lineBuilder.Length = 0;

        //        // Build line numbers string
        //        for (int i = 1; i < currentLineCount + 2; i++)
        //        {
        //            if (i - 1 > 0 && i - 1 < currentLineCount - 1)
        //            {
        //                int characterStart = inputText.textInfo.lineInfo[i - 1].firstCharacterIndex;
        //                int characterCount = inputText.textInfo.lineInfo[i - 1].characterCount;

        //                if (characterCount != 0 && inputText._text.Substring(characterStart, characterCount).Contains("\n") == false)
        //                {
        //                    lineBuilder.Append("\n");
        //                    continue;
        //                }
        //            }

        //            lineBuilder.Append(currentLineNumber);
        //            lineBuilder.Append('\n');

        //            currentLineNumber++;

        //            if (i - 1 == 0 && i - 1 < currentLineCount - 1)
        //            {
        //                int characterStart = inputText.textInfo.lineInfo[i - 1].firstCharacterIndex;
        //                int characterCount = inputText.textInfo.lineInfo[i - 1].characterCount;

        //                if (characterCount != 0 && inputText._text.Substring(characterStart, characterCount).Contains("\n") == false)
        //                {
        //                    lineBuilder.Append("\n");
        //                    continue;
        //                }
        //            }
        //        }

        //        // Update displayed line numbers
        //        lineText._text = lineBuilder.ToString();
        //        lineCount = currentLineCount;
        //    }
        //}

        // Override the caret position calculation to account for _beforeText
        private void UpdateCurrentLineColumnIndent()
        {
            // Calculate the actual position including the _beforeText
            int totalPosition = beforeText.Length + inputField.caretPosition;

            // Count lines up to this position
            int lineCount = 0;
            int lastNewLine = -1;

            string fullText = Text;
            for (int i = 0; i < totalPosition; i++)
            {
                if (fullText[i] == '\n')
                {
                    lineCount++;
                    lastNewLine = i;
                }
            }

            currentLine = lineCount;
            currentColumn = totalPosition - (lastNewLine + 1);

            // Update indent level if needed
            if (languageTheme != null && languageTheme.autoIndent.allowAutoIndent)
            {
                currentIndent = 0;
                for (int i = 0; i < totalPosition; i++)
                {
                    if (fullText[i] == languageTheme.autoIndent.indentIncreaseCharacter)
                        currentIndent++;
                    if (fullText[i] == languageTheme.autoIndent.indentDecreaseCharacter)
                        currentIndent--;
                }

                if (currentIndent < 0)
                    currentIndent = 0;
            }
        }


        private void UpdateCurrentLineHighlight()
        {
            // Check if code editor is not active or line highlighting is locked
            if (!isActiveAndEnabled || lineHighlightLocked)
                return;

            // Ensure inputText and inputText.textInfo are valid before proceeding
            if (inputText == null || inputText.textInfo == null || inputText.textInfo.characterInfo.Length == 0)
            {
                // If there is no text, reset the highlight position or hide it
                lineHighlightTransform.anchoredPosition = new Vector2(5, 0);
                return;
            }

            int lineOffset = 0;

#if UNITY_2018_2_OR_NEWER
            if (applyLineOffsetFix)
                lineOffset++;
#endif

            // Compute the Y offset using beforeInputText's height
            float beforeInputOffset = beforeInputText != null ? beforeInputText.preferredHeight : 0f;

            // Get the current line number, ensuring caretPosition is within bounds
            int caretPosition = Mathf.Clamp(inputField.caretPosition, 0, inputText.textInfo.characterInfo.Length - 1);
            int currentLine = inputText.textInfo.characterInfo[caretPosition].lineNumber;

            // Highlight the current line
            lineHighlightTransform.anchoredPosition = new Vector2(
                5,
                inputText.textInfo.lineInfo[currentLine].lineHeight *
                (-currentLine + lineOffset) -
                _perLineHightlightOffset +
                inputTextTransform.anchoredPosition.y -
                beforeInputOffset // Apply the Y offset
            );
        }


        private string SyntaxHighlightContent(string inputText)
        {
            // Check if parsing should not run
            if (languageTheme == null)
                return inputText;

            // Check if the theme supports highlighting
            if (editorTheme != null && editorTheme.allowSyntaxHighlighting == false)
                return inputText;

            const string closingTag = "</color>";
            int offset = 0;

            highlightedBuilder.Length = 0;

            foreach (InputStringMatchInfo match in lexer.LexInputString(inputText))
            {
                // Copy _text before the match
                for (int i = offset; i < match.startIndex; i++)
                    highlightedBuilder.Append(inputText[i]);

                // Add the opening color tag
                highlightedBuilder.Append(match.htmlColor);

                // Copy _text inbetween the match boundaries
                for (int i = match.startIndex; i < match.endIndex; i++)
                    highlightedBuilder.Append(inputText[i]);

                // Add the closing color tag
                highlightedBuilder.Append(closingTag);

                // Update offset
                offset = match.endIndex;
            }

            // Copy remaining _text
            for (int i = offset; i < inputText.Length; i++)
                highlightedBuilder.Append(inputText[i]);

            // Convert to string
            inputText = highlightedBuilder.ToString();

            return inputText;
        }

        private void AutoIndentCaret(bool isClosingToken = false)
        {
            // Check for new line
            if (Input.GetKeyDown(KeyCode.Return) == true)
            {
                // Update line column and indent positions
                UpdateCurrentLineColumnIndent();

                // Build indent string
                string indent = string.Empty;

                // Make sure we have some _text
                if (inputField.caretPosition < inputField.text.Length)
                {
                    // Check for tabs before caret
                    int beforeIndentCount = 0;
                    int caretIndentCount = 0;
                    for (int i = inputField.caretPosition; i >= 0; i--)
                    {
                        // Check for tab characters
                        if (inputField.text[i] == '\t')
                            beforeIndentCount++;

                        // Check for previous line or spaces
                        if (inputField.text[i] == '\n' || (languageTheme.autoIndent.autoIndentMode == AutoIndent.IndentMode.AutoTabContextual && inputField.text[i] != ' '))
                            if (i != inputField.caretPosition)
                                break;
                    }

                    if (languageTheme.autoIndent.autoIndentMode == AutoIndent.IndentMode.AutoTabContextual)
                    {
                        // Take into account any previous tab characters
                        caretIndentCount = currentIndent - caretIndentCount;
                    }
                    else
                    {
                        caretIndentCount = beforeIndentCount;
                    }

                    indent = GetAutoIndentTab(caretIndentCount);

                    //int length = 0;

                    //for(int i = inputField.caretPosition + 1; i < inputField._text.Length; i++, length++)
                    //{
                    //    if (inputField._text[i] == '\n')
                    //        break;
                    //}

                    //int caret = 0;
                    //string formatted = languageTheme.autoIndent.GetAutoIndentedFormattedString(inputField._text.Substring(inputField.caretPosition + 1, length), currentIndent, out caret);

                    //inputField._text = inputField._text.Remove(inputField.caretPosition + 1, length);
                    //inputField._text = inputField._text.Insert(inputField.caretPosition + 1, formatted);

                    //inputField.stringPosition = inputField.stringPosition + caret;
                    //return;
                }


                if (indent.Length > 0)
                {
                    // Get caret position
                    inputField.text = inputField.text.Insert(inputField.caretPosition + 1, indent);

                    // Move to the end of the new line
                    inputField.stringPosition = inputField.stringPosition + indent.Length;
                }

                //if (languageTheme.autoIndent.autoIndentMode == AutoIndent.IndentMode.AutoTabContextual)
                //{
                // Check for closing bracket
                bool immediateClosing = false;
                int closingOffset = -1;

                for (int i = inputField.caretPosition + 1; i < inputField.text.Length; i++)
                {
                    if (inputField.text[i] == languageTheme.autoIndent.indentDecreaseCharacter)
                    {
                        // Set the closing flag
                        immediateClosing = true;
                        closingOffset = i - (inputField.caretPosition + 1);
                        break;
                    }

                    // Check for any other character
                    if (char.IsWhiteSpace(inputField.text[i]) == false || inputField.text[i] == '\n')
                        break;
                }

                if (immediateClosing == true)
                {
                    // Remove unnecessary white space
                    inputField.text = inputField.text.Remove(inputField.caretPosition + 1, closingOffset);

                    string localIndent = (string.IsNullOrEmpty(indent) == true) ? string.Empty : indent.Remove(0, 1);


                    // Insert new line
                    inputField.text = inputField.text.Insert(inputField.caretPosition + 1, GetAutoIndentTab(currentIndent) + "\n" + localIndent);

                    //inputField.stringPosition -= 1;

                    if (string.IsNullOrEmpty(localIndent) == false)
                    {
                        //inputField.stringPosition -= localIndent.Length;
                    }


                    // Update line column and indent positions
                    UpdateCurrentLineColumnIndent();
                }
            }
            //}

            // Check for closing token
            if (isClosingToken == true)
            {
                if (inputField.caretPosition > 0)
                {
                    // Check for tab before caret
                    if (inputField.text[inputField.caretPosition - 1] == '\t')
                    {
                        // Remove 1 tab because we have received a closing token
                        inputField.text = inputField.text.Remove(inputField.caretPosition - 1, 1);

                        inputField.stringPosition = inputField.stringPosition - 1;
                    }
                }
            }

            inputText.text = inputField.text;
            inputText.SetText(inputField.text, true);
            inputText.Rebuild(CanvasUpdate.Prelayout);
            inputField.ForceLabelUpdate();
            inputField.Rebuild(CanvasUpdate.Prelayout);
            Refresh(true);
            delayedRefresh = true;
        }

        private string GetAutoIndentTab(int amount)
        {
            string tab = string.Empty;

            for (int i = 0; i < amount; i++)
                tab += '\t';

            return tab;
        }

        private void ApplyTheme()
        {
            // Check for missing references
            if (AllReferencesAssigned() == false)
                throw new MissingReferenceException("Cannot apply theme because one or more required component references are missing. Make sure all references under the 'Elements' header are assigned");

            bool nullTheme = false;

            // Check for no theme
            if (editorTheme == null)
            {
                // Get the default theme
                editorTheme = CodeEditorTheme.DefaultTheme;
                nullTheme = true;
            }

            // Apply theme colors
            inputField.caretColor = editorTheme.caretColor;
            //inputText.color = editorTheme.textColor;
            inputText.color = Color.clear;
            inputHighlightText.color = editorTheme.textColor;
            background.color = editorTheme.backgroundColor;
            lineHighlight.color = editorTheme.lineHighlightColor;
            lineNumberBackground.color = editorTheme.lineNumberBackgroundColor;
            lineText.color = editorTheme.lineNumberTextColor;
            scrollbar.color = editorTheme.scrollbarColor;

            // Apply to new sections
            beforeInputText.color = Color.clear;
            beforeInputHighlightText.color = editorTheme.textColor;
            afterInputText.color = Color.clear;
            afterInputHighlightText.color = editorTheme.textColor;

            // Set active to null
            if (nullTheme == true)
                editorTheme = null;
        }

        private void ApplyLanguage()
        {
            // Check for no theme
            char[] delimiters = null;
            MatchLexer[] matchers = null;

            // Get the matchers for the theme
            if (languageTheme != null)
            {
                delimiters = languageTheme.DelimiterSymbols;
                matchers = languageTheme.Matchers;
            }

            // Apply theme matchers
            lexer.UseMatchers(delimiters, matchers);
        }

        private bool AllReferencesAssigned()
        {
            if (inputField == null ||
                inputText == null ||
                inputHighlightText == null ||
                lineText == null ||
                background == null ||
                lineHighlight == null ||
                lineNumberBackground == null ||
                scrollbar == null)
            {
                // One or more references are not assigned
                return false;
            }
            return true;
        }

        [SerializeField]
        GameObject _lineRunningHighlight;
        [SerializeField]
        [Tooltip("Prevents making more '}' symbols than '{' symbols in the input field")]
        private bool _disableClosingBracketPrevention;

        internal void HightlightRunningLine(int lineNr)
        {
            // Check if code editor is not active or line number is invalid
            //Debug.Log("LC:" + LineCount);

            if (lineNr < 1 || lineNr > LineCount)
                return;

            int lineOffset = 0;
            int lineIndex = lineNr - 1; // Convert to 0-based index

#if UNITY_2018_2_OR_NEWER
            if (applyLineOffsetFix == true)
                lineOffset++;
#endif

            // Make sure we have a running line highlight
            if (_lineRunningHighlight == null)
            {
                return;
            }
            //Debug.Log("LINE");
            // Position the highlight at the specified line
            RectTransform runningHighlightTransform = _lineRunningHighlight.GetComponent<RectTransform>();
            runningHighlightTransform.anchoredPosition = new Vector2(5,
                (inputText.textInfo.lineInfo[inputText.textInfo.characterInfo[0].lineNumber].lineHeight *
                -lineIndex) + lineOffset - _perLineHightlightOffset +
                inputTextTransform.anchoredPosition.y);

            // Make sure it's visible
            _lineRunningHighlight.gameObject.SetActive(true);
        }

        internal void DisableRunningLine()
        {
            _lineRunningHighlight.gameObject.SetActive(false);
        }
    }

}
