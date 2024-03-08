namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        // State variables to manage calculator logic and UI
        double lastNumber = 0;
        string currentOperation = "";
        bool newOperation = true;
        bool operationPerformed = false;
        bool inErrorState = false;
        double lastInputNumber = 0;
        bool resultDisplayed = false;
        Button selectedOperationButton = null;

        public MainPage()
        {
            InitializeComponent();
            AssignButtonEvents(); // Attach event handlers dynamically to buttons
        }

        // Handles numeric and decimal point button clicks
        private void OnSelectNumber(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string pressed = button.Text;
            int maxDigits = 9; // Limit the number of digits to prevent overflow

            // Reset state if in error or after a result was displayed
            if (inErrorState || resultDisplayed)
            {
                ResetCalculatorState();
            }

            // Special handling for "-0" state
            if (Display.Text == "-0")
            {
                Display.Text = "-" + pressed;
                newOperation = false;
                operationPerformed = false;
                return;
            }

            // Input validation to prevent invalid number formats
            if ((Display.Text.Contains(".") && pressed == ".") || (Display.Text == "0" && pressed == "0" && !newOperation))
            {
                return; // Do nothing if invalid input is detected
            }

            // Append number or replace display text based on the state
            if ((Display.Text.Replace(".", "").Length < maxDigits || newOperation) && !(Display.Text.Contains(".") && pressed == "."))
            {
                Display.Text = Display.Text == "0" || newOperation ? pressed == "." ? "0." : pressed : Display.Text += pressed;
                newOperation = false;
            }

            operationPerformed = false;

            // Reset operation button style if another operation is selected
            if (selectedOperationButton != null)
            {
                ResetOperationButtonStyle(selectedOperationButton);
                selectedOperationButton = null;
            }

            AdjustDisplayFontSize(); // Adjust font size based on the display content length
        }

        // Handles operation (+, -, *, /) button clicks
        private void OnSelectOperator(object sender, EventArgs e)
        {
            if (inErrorState) return; // Ignore if in error state

            Button button = (Button)sender;
            if (selectedOperationButton != null && selectedOperationButton != button)
            {
                ResetOperationButtonStyle(selectedOperationButton); // Reset style of previously selected operation
            }

            selectedOperationButton = button;
            button.Style = (Style)Application.Current.Resources["OperationButtonSelected"]; // Highlight the selected operation

            // Perform calculation or setup for new operation
            if (!newOperation && !operationPerformed && !resultDisplayed)
            {
                double displayNumber = double.Parse(Display.Text);
                if (currentOperation != "")
                {
                    CalculateResult(displayNumber);
                }
                else
                {
                    lastNumber = displayNumber;
                }
            }
            else if (resultDisplayed)
            {
                lastNumber = double.Parse(Display.Text);
                resultDisplayed = false;
            }

            currentOperation = button.Text;
            newOperation = true;
            operationPerformed = false;
        }

        // Executes the calculation based on the selected operation and input numbers
        private void OnCalculate(object sender, EventArgs e)
        {
            if (inErrorState || currentOperation == "" && !operationPerformed) return; // Prevent calculation in invalid states

            double newNumber = operationPerformed ? lastInputNumber : double.Parse(Display.Text); // Use last input if repeating operation
            CalculateResult(newNumber); // Perform calculation
            lastInputNumber = newNumber; // Store last input for repeat operations
            operationPerformed = true;
            resultDisplayed = true;

            // Reset operation button style
            if (selectedOperationButton != null)
            {
                ResetOperationButtonStyle(selectedOperationButton);
                selectedOperationButton = null;
            }

            AdjustDisplayFontSize(); // Adjust font size for new result
        }

        // Core calculation logic based on the current operation
        private void CalculateResult(double newNumber)
        {
            if (currentOperation == "") return; // Exit if no operation is selected

            double result = 0;
            switch (currentOperation)
            {
                case "+":
                    result = lastNumber + newNumber;
                    break;
                case "-":
                    result = lastNumber - newNumber;
                    break;
                case "x":
                    result = lastNumber * newNumber;
                    break;
                case "÷":
                    if (newNumber == 0)
                    {
                        Display.Text = "Error"; // Handle divide by zero error
                        inErrorState = true;
                        return;
                    }
                    result = lastNumber / newNumber;
                    break;
            }

            // Round the result before displaying it
            // Adjust the precision as needed
            result = Math.Round(result, 15); // Rounds the result to 15 decimal places

            Display.Text = result.ToString();
            lastNumber = result; // Store result for next operation
            newOperation = true; // Ready for new operation
        }

        // Resets calculator to its initial state
        private void OnClear(object sender, EventArgs e)
        {
            ResetCalculatorState(); // Reset state
            AdjustDisplayFontSize(); // Adjust font size back to default
        }

        // Toggles the sign of the current number
        private void OnChangeSign(object sender, EventArgs e)
        {
            if (inErrorState) return; // Ignore if in error state

            // Toggle sign logic
            switch (Display.Text)
            {
                case "0":
                    Display.Text = "-0";
                    break;
                case "-0":
                    Display.Text = "0";
                    break;
                default:
                    if (newOperation && !operationPerformed)
                    {
                        Display.Text = "-0";
                        newOperation = false;
                    }
                    else
                    {
                        double number = double.Parse(Display.Text);
                        number *= -1;
                        Display.Text = number.ToString();
                    }
                    break;
            }
        }

        // Converts the current number into a percentage
        private void OnPercentage(object sender, EventArgs e)
        {
            if (inErrorState) return; // Ignore if in error state

            double number = double.Parse(Display.Text);

            // Adjust percentage calculation based on the operation
            if (!newOperation && (currentOperation == "+" || currentOperation == "-"))
            {
                // For addition and subtraction, calculate percentage of lastNumber
                number = (lastNumber * number) / 100;
            }
            else
            {
                // For multiplication and division, or no operation, calculate simple percentage
                number /= 100;
            }

            Display.Text = number.ToString();
            if (currentOperation == "" || newOperation)
            {
                lastNumber = number; // Update lastNumber if no current operation or starting new operation
            }
            newOperation = true; // Ready for new operation
        }

        // Handles Delete button pressed left gesture to delete the last digit
        private void OnDelete(object sender, EventArgs e)
        {
            if (Display.Text.Length <= 1 || inErrorState)
            {
                Display.Text = "0";
                newOperation = true;
            }
            else
            {
                Display.Text = Display.Text.Remove(Display.Text.Length - 1); // Remove last digit
                AdjustDisplayFontSize(); // Adjust font size if necessary
            }
        }


        // Handles swipe left gesture to delete the last digit
        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (Display.Text.Length <= 1 || inErrorState)
            {
                Display.Text = "0";
                newOperation = true;
            }
            else
            {
                Display.Text = Display.Text.Remove(Display.Text.Length - 1); // Remove last digit
                AdjustDisplayFontSize(); // Adjust font size if necessary
            }
        }

        // Resets all state variables and UI elements to initial state
        private void ResetCalculatorState()
        {
            lastNumber = 0;
            currentOperation = "";
            lastInputNumber = 0;
            Display.Text = "0";
            newOperation = true;
            operationPerformed = false;
            inErrorState = false;
            resultDisplayed = false;

            // Reset style of the selected operation button, if any
            if (selectedOperationButton != null)
            {
                ResetOperationButtonStyle(selectedOperationButton);
                selectedOperationButton = null;
            }
        }

        // Dynamically attach press and release events to all buttons
        private void AssignButtonEvents()
        {
            foreach (var view in this.Content.FindByName<Grid>("ButtonGrid").Children)
            {
                if (view is Button button)
                {
                    button.Pressed += OnButtonPressed;
                    button.Released += OnButtonReleased;
                }
            }
        }

        // Applies a visual feedback style when a button is pressed
        private void OnButtonPressed(object sender, EventArgs e)
        {
            var button = (Button)sender;
            ApplyPressedStyle(button); // Apply the pressed style
        }

        // Restores the button's style upon release, with a fade effect
        private async void OnButtonReleased(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await button.FadeTo(1, 100); // Fade effect for visual feedback
            if (button != selectedOperationButton)
            {
                ResetButtonStyle(button); // Reset style if the button is not the selected operation
            }
        }

        // Applies the "pressed" style based on the button type
        private void ApplyPressedStyle(Button button)
        {
            var key = button.Style == (Style)Application.Current.Resources["NumberButton"] ? "NumberButtonPressed" :
                      button.Style == (Style)Application.Current.Resources["FunctionButton"] ? "FunctionButtonPressed" :
                      "OperationButtonPressed";
            button.Style = (Style)Application.Current.Resources[key];
        }

        // Resets the button style to its default based on the button type
        private void ResetButtonStyle(Button button)
        {
            if (button != selectedOperationButton)
            {
                var key = button.Text.All(char.IsDigit) || button.Text == "." ? "NumberButton" :
                          button.Text == "AC" || button.Text == "+/-" || button.Text == "%" || button.Text == "DEL" ? "FunctionButton" :
                          "OperationButton";
                button.Style = (Style)Application.Current.Resources[key];
            }
        }

        // Resets the operation button style to unselected
        private void ResetOperationButtonStyle(Button button)
        {
            button.Style = (Style)Application.Current.Resources["OperationButton"];
        }

        // Adjusts the display font size based on the length of the text
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            AdjustDisplayFontSize(); // Adjust display font size dynamically
        }

        // Dynamically adjusts the font size of the display to fit the available space
        private void AdjustDisplayFontSize()
        {
            double maxWidth = Display.Width - 20; // Consider margins
            Display.FontSize = 60; // Start with a base font size

            double characterWidth = Display.FontSize * 0.6; // Estimate character width

            int maxCharacters = (int)(maxWidth / characterWidth); // Calculate max number of characters that can fit

            // Reduce font size until the text fits or reaches minimum font size
            while (Display.Text.Length > maxCharacters && Display.FontSize > 20)
            {
                Display.FontSize -= 2;
                characterWidth = Display.FontSize * 0.6;
                maxCharacters = (int)(maxWidth / characterWidth);
            }
        }
    }
}
