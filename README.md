# SerialLogger

*SerialLogger* is an application for receiving and saving serial data from Arduino devices.
It was developed in C# WPF on .NET 8.0 with the MVVM Toolkit.
When the program is started, choose the serial port and baud rate and click the **Open port** button to start receiving data. 

## Files

When the **Save to file** option is enabled, serial data is saved to a specified folder. *SerialLogger* will store around 1000 lines of data per file before starting a new one.

Data files are saved with a *.txt* extension and are named using the current date and time, followed by a three-character random suffix.

## License

Feel free to use this project as you wish, as long as you credit the original author and the GitHub repository.


## Credits

Author: Djordje Herceg

Date: November 2024

GitHub repository: https://github.com/djherceg/SerialLogger1