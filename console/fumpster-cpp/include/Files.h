#ifndef FILES_H
#define FILES_H
#include <iostream>
#include <fstream>
#include <filesystem>


namespace Fumpster::Files
{
/// <summary>
/// DumpedFile (14/12/2024)
/// by volodymyr-tsukanov
/// </summary>
class DumpedFile {
public:
    enum Status {NEW, NORMAL, OLD};

protected:
    const short REPUTATION_RESTORED = -7098, REPUTATION_DUMPED_DEFAULT = 10, REPUTATION_MAX = 100;
    const char separator = ';'; // Separator for the data fields

private:
    long id;
    short reputation;
    std::string path;

public:
    // Constructor to initialize with data using stringstream
    DFile(std::string data) {
        std::stringstream ss(data);
        std::string temp;

        // Read id, reputation, and path from the stringstream
        std::getline(ss, temp, separator);  // Read id as string and convert to long
        id = std::stol(temp);

        std::getline(ss, temp, separator);  // Read reputation as string and convert to short
        reputation = static_cast<short>(std::stoi(temp));

        std::getline(ss, path);  // Remaining part is the file path
    }

    // Save the file by appending a new line to the specified file path
    bool save(std::string filePath) {
        std::ofstream file(filePath, std::ios::app); // Open file in append mode
        if (!file.is_open()) {
            return false; // Return false if the file cannot be opened
        }

        // Save the DFile data to the file as a new line with the defined separator
        file << id << separator << reputation << separator << path << std::endl;

        file.close(); // Close the file
        return true;  // Return true if saving was successful
    }
};
}

#endif // FILES_H
