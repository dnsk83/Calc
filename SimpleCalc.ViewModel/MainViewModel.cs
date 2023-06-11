using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Security;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SimpleCalc.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly string _decimalSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;

        private string _displayValue;
        private bool _isDisplayingResult;
        private bool _decimalSeparatorAwailable;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayValue 
        {
            get => _displayValue;
            private set
            {
                if (value != _displayValue)
                {
                    _displayValue = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand NumberCommand { get; private set; }
        public ICommand DecimalSeparatorCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand BackspaceCommand { get; private set; }
        public ICommand OperatorCommand { get; private set; }
        public ICommand EvaluateCommand { get; private set; }

        public MainViewModel()
        {
            NumberCommand = new Command(NumberCommandExecute);
            DecimalSeparatorCommand = new Command(DecimalSeparatorCommandExecute);
            ResetCommand = new Command(Reset);
            BackspaceCommand = new Command(BackspaceCommandExecute);
            OperatorCommand = new Command(OperatorCommandExecute);
            EvaluateCommand = new Command(EvaluateCommandExecute);

            Reset(null);
        }

        private void Reset(object obj = null)
        {
            DisplayValue = "0";
            ResetIsDisplayingResult();
            _decimalSeparatorAwailable = true;
        }

        private void NumberCommandExecute(object obj)
        {
            if (_isDisplayingResult)
            {
                DisplayValue = "";
                ResetIsDisplayingResult();
            }

            var str = obj.ToString();

            if (DisplayValue == "0")
            {
                if (str == "0") return;
                DisplayValue = str;
            }
            else
            {
                DisplayValue += str;
            }
        }

        private void BackspaceCommandExecute(object obj)
        {
            if (DisplayValue.Length > 1)
            {
                if (LastSymbolIsDecimalSeparator()) 
                { 
                    _decimalSeparatorAwailable = true; 
                }
                DisplayValue = DisplayValue.Remove(DisplayValue.Length - 1);
            }
            else
            {
                DisplayValue = "0";
            }
            ResetIsDisplayingResult();
        }

        private void DecimalSeparatorCommandExecute(object obj)
        {
            if(_isDisplayingResult)
            {
                Reset();
            }

            if (_decimalSeparatorAwailable)
            {
                // when user enters smth like ".12" to get "0.12"
                if (LastSymbolIsOperator() || IsEmpty())
                {
                    DisplayValue += "0";
                }
                DisplayValue += _decimalSeparator;
                _decimalSeparatorAwailable = false;
            }
        }

        private void OperatorCommandExecute(object obj)
        {
            if (CanAddOperator())
            {
                ResetIsDisplayingResult();
                DisplayValue += obj.ToString();
                _decimalSeparatorAwailable = true;
            }
        }

        private void EvaluateCommandExecute(object obj)
        {
            if (!IsEmpty() && !_isDisplayingResult)
            {
                if (LastSymbolIsOperator() || LastSymbolIsDecimalSeparator())
                {
                    DisplayValue = DisplayValue.Remove(DisplayValue.Length - 1);
                }
                DisplayValue = Evaluate(DisplayValue);
                _isDisplayingResult = true;
                _decimalSeparatorAwailable = true;
            }
        }

        private string Evaluate(string expression)
        {
            var resultStr = new System.Data.DataTable().Compute(expression, null).ToString();
            if (double.TryParse(resultStr , out var resultNumber))
            {
                return resultNumber.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return resultStr; // Do I really have to handle decimal separator if failed to parse?
            }
        }

        void ResetIsDisplayingResult()
        {
            _isDisplayingResult = false;
        }

        private bool CanAddOperator()
        {
            if (IsEmpty()) return false;
            if (LastSymbolIsOperator()) return false;
            return true;
        }

        private bool LastSymbolIsOperator()
        {
            return Regex.IsMatch(DisplayValue[DisplayValue.Length - 1].ToString(), @"[+\-*/]");
        }

        private bool LastSymbolIsDecimalSeparator()
        {
            return DisplayValue[DisplayValue.Length - 1].ToString() == _decimalSeparator;
        }

        private bool IsEmpty()
        {
            return DisplayValue.Length == 0;
        }

        void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
