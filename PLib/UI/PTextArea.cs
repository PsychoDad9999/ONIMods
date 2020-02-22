﻿/*
 * Copyright 2020 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using PeterHan.PLib.UI.Layouts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeterHan.PLib.UI {
	/// <summary>
	/// A custom UI text area (multi-line text field) factory class. This class should
	/// probably be wrapped in a scroll pane.
	/// </summary>
	public sealed class PTextArea : IUIComponent {
		/// <summary>
		/// The text area's background color.
		/// </summary>
		public Color BackColor { get; set; }

		/// <summary>
		/// The flexible size bounds of this component.
		/// </summary>
		public Vector2 FlexSize { get; set; }

		/// <summary>
		/// The preferred number of text lines to be displayed. If the component is made
		/// bigger, the number of text lines (and size) can increase.
		/// </summary>
		public int LineCount { get; set; }

		/// <summary>
		/// The maximum number of characters in this text area.
		/// </summary>
		public int MaxLength { get; set; }

		public string Name { get; }

		/// <summary>
		/// The minimum width in units (not characters!) of this text area.
		/// </summary>
		public int MinWidth { get; set; }

		/// <summary>
		/// The text alignment in the text area.
		/// </summary>
		public TextAlignmentOptions TextAlignment { get; set; }

		/// <summary>
		/// The initial text in the text field.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The text field's text color, font, word wrap settings, and font size.
		/// </summary>
		public TextStyleSetting TextStyle { get; set; }

		/// <summary>
		/// The tool tip text.
		/// </summary>
		public string ToolTip { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		/// <summary>
		/// The action to trigger on text change. It is passed the realized source object.
		/// </summary>
		public PUIDelegates.OnTextChanged OnTextChanged { get; set; }

		/// <summary>
		/// The callback to invoke when validating input.
		/// </summary>
		public TMP_InputField.OnValidateInput OnValidate { get; set; }

		public PTextArea() : this(null) { }

		public PTextArea(string name) {
			BackColor = PUITuning.Colors.BackgroundLight;
			FlexSize = Vector2.one;
			LineCount = 4;
			MaxLength = 1024;
			MinWidth = 64;
			Name = name ?? "TextArea";
			Text = null;
			TextAlignment = TextAlignmentOptions.TopLeft;
			TextStyle = PUITuning.Fonts.TextDarkStyle;
			ToolTip = "";
		}

		public GameObject Build() {
			var textField = PUIElements.CreateUI(null, Name);
			// Background
			var style = TextStyle ?? PUITuning.Fonts.TextLightStyle;
			textField.AddComponent<Image>().color = style.textColor;
			// Text box with rectangular clipping area; put pivot in upper left
			var textArea = PUIElements.CreateUI(textField, "Text Area", false);
			textArea.rectTransform().pivot = Vector2.up;
			textArea.AddComponent<Image>().color = BackColor;
			var mask = textArea.AddComponent<RectMask2D>();
			// Scrollable text
			var textBox = PUIElements.CreateUI(textArea, "Text", true, PUIAnchoring.Beginning,
				PUIAnchoring.End);
			// Text to display
			var textDisplay = textBox.AddComponent<TextMeshProUGUI>();
			textDisplay.alignment = TextAlignment;
			textDisplay.autoSizeTextContainer = false;
			textDisplay.enabled = true;
			textDisplay.color = style.textColor;
			textDisplay.font = style.sdfFont;
			textDisplay.fontSize = style.fontSize;
			textDisplay.fontStyle = style.style;
			// Text field itself
			textField.SetActive(false);
			var textEntry = textField.AddComponent<TMP_InputField>();
			textEntry.textComponent = textDisplay;
			textEntry.textViewport = textArea.rectTransform();
			textField.SetActive(true);
			textEntry.text = Text ?? "";
			textDisplay.text = Text ?? "";
			// Events!
			ConfigureTextEntry(textEntry);
			var events = textField.AddComponent<PTextFieldEvents>();
			events.OnTextChanged = OnTextChanged;
			events.OnValidate = OnValidate;
			// Add tooltip
			if (!string.IsNullOrEmpty(ToolTip))
				textField.AddComponent<ToolTip>().toolTip = ToolTip;
			mask.enabled = true;
			// Lay out - TMP_InputField does not support auto layout but we do!
			var element = textField.AddOrGet<PTextAreaLayout>();
			PUIElements.SetAnchorOffsets(textArea, 1.0f, 1.0f, 1.0f, 1.0f);
			element.minWidth = MinWidth;
			element.CalculatedHeight = Math.Max(LineCount, 1) * PUIUtils.GetLineHeight(style);
			element.flexibleHeight = FlexSize.y;
			element.flexibleWidth = FlexSize.x;
			OnRealize?.Invoke(textField);
			return textField;
		}

		/// <summary>
		/// Sets up the text entry field.
		/// </summary>
		/// <param name="textEntry">The input field to configure.</param>
		private void ConfigureTextEntry(TMP_InputField textEntry) {
			textEntry.characterLimit = Math.Max(1, MaxLength);
			textEntry.enabled = true;
			textEntry.inputType = TMP_InputField.InputType.Standard;
			textEntry.interactable = true;
			textEntry.isRichTextEditingAllowed = false;
			textEntry.keyboardType = TouchScreenKeyboardType.Default;
			textEntry.lineType = TMP_InputField.LineType.MultiLineNewline;
			textEntry.navigation = Navigation.defaultNavigation;
			textEntry.richText = false;
			textEntry.selectionColor = PUITuning.Colors.SelectionBackground;
			textEntry.transition = Selectable.Transition.None;
			textEntry.restoreOriginalTextOnEscape = true;
		}

		/// <summary>
		/// Sets the default Klei pink style as this text area's color and text style.
		/// </summary>
		/// <returns>This button for call chaining.</returns>
		public PTextArea SetKleiPinkStyle() {
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			return this;
		}

		/// <summary>
		/// Sets the default Klei blue style as this text area's color and text style.
		/// </summary>
		/// <returns>This button for call chaining.</returns>
		public PTextArea SetKleiBlueStyle() {
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			return this;
		}

		/// <summary>
		/// Sets the minimum (and preferred) width of this text area in characters.
		/// 
		/// The width is computed using the currently selected text style.
		/// </summary>
		/// <param name="chars">The number of characters to be displayed.</param>
		/// <returns>This button for call chaining.</returns>
		public PTextArea SetMinWidthInCharacters(int chars) {
			int width = Mathf.RoundToInt(chars * PUIUtils.GetEmWidth(TextStyle));
			if (width > 0)
				MinWidth = width;
			return this;
		}

		public override string ToString() {
			return "PTextArea[Name={0}]".F(Name);
		}

		/// <summary>
		/// Handles layout for text areas. Not freezable.
		/// </summary>
		private sealed class PTextAreaLayout : AbstractTextFieldLayout {
			protected override float BorderSize => 1.0f;

			/// <summary>
			/// The calculated height of the text box according to the line count set.
			/// </summary>
			internal float CalculatedHeight { get; set; }

			public override void CalculateLayoutInputVertical() {
#pragma warning disable IDE0031 // Use null propagation
				var child = (textBox == null) ? null : textBox.rectTransform();
#pragma warning restore IDE0031
				if (textArea != null && calcElements != null) {
					// Lay out children
					foreach (var component in calcElements)
						if (!PUIUtils.IgnoreLayout(component))
							component.CalculateLayoutInputVertical();
					calcElements = null;
				}
				if (child != null) {
					float height = CalculatedHeight;
					if (height.IsNaNOrInfinity() || height <= 0.0f)
						height = LayoutUtility.GetPreferredHeight(child);
					// 1px for the border
					minHeight = preferredHeight = height + 2.0f;
				} else
					// Fallback if text box is somehow not set
					minHeight = preferredHeight = 1.0f;
			}
		}
	}
}