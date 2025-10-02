using System;                      // Provides basic system functionality (Console, Math, etc.)
using System.Collections.Generic;  // Provides the List<T> collection type
using System.IO;                   // Provides file read/write support
using System.Linq;                 // Provides LINQ methods like Select, Average, Sum, Aggregate

/* 
 * This program reads a stock list of cars from a CSV file,
 * processes them into Car objects, and then highlights:
 * - The cheapest car available
 * - The average price of cars
 * - The average mileage of cars
 * - The total number of cars for sale
 * - The total stock value of all cars combined
 */

class Program
{
    // Car Class definition
    // Holds the data for each car entry from the CSV
    public class Car
    {
        public string Registration { get; set; } // Car registration number
        public string Make { get; set; }         // Car manufacturer (e.g Toyota, Ford)
        public string Model { get; set; }        // Car model (e.g Corolla, Fiesta)
        public int Mileage { get; set; }         // Mileage in kilometers
        public int Price { get; set; }           // Price in Euros
    }
    
    /// <summary>
    /// Calculates the average numeric value from a list of cars,
    /// based on the property selected (e.g., Mileage, Price).
    /// 
    /// Uses a lambda function (selector) to tell which property
    /// should be averaged. For example: car => car.Price means
    /// "take the Price property of each car".
    /// </summary>
    static double CalculateAverage<T>(List<Car> cars, Func<Car, T> selector) where T : struct, IConvertible
    {
        // If there are no cars, avoid errors and just return 0
        if (cars == null || cars.Count == 0)
            return 0;

        // Select() uses the selector (lambda) to extract the chosen property from each car
        // Convert.ToDouble ensures all numbers are turned into doubles for greater precision
        var values = cars.Select(c => Convert.ToDouble(selector(c)));

        // Average() is a LINQ method that computes the mean of the sequence
        return values.Average();
    }

    /// <summary>
    /// Finds the cheapest car in the list.
    /// Uses the Aggregate() LINQ method to compare cars one by one
    /// and keep the one with the lowest price.
    /// </summary>
    static Car GetCheapestCar(List<Car> cars)
    {
        if (cars == null || cars.Count == 0)
            return null;

        // Aggregate runs through the list and keeps the car with the lowest Price
        return cars.Aggregate((minCar, nextCar) => nextCar.Price < minCar.Price ? nextCar : minCar);
        // Explanation of the lambda:
        // (minCar, nextCar) => nextCar.Price < minCar.Price ? nextCar : minCar
        // - If nextCar is cheaper, keep it
        // - Otherwise, keep minCar
    }

    /// <summary>
    /// Provides a summary of total cars and their total price value.
    /// Uses a tuple: (int totalCars, int totalValue),
    /// which allows returning more than one value without a custom class.
    /// </summary>
    static (int totalCars, int totalValue) GetStockSummary(List<Car> cars)
    {
        if (cars == null || cars.Count == 0)
            return (0, 0);

        int totalCars = cars.Count;            // Total number of cars
        int totalValue = cars.Sum(car => car.Price); // LINQ Sum() adds up all prices
        return (totalCars, totalValue);        // Return tuple
    }

    // Main entry point where the program starts execution
    static void Main()
    {
        bool debug = true; // Debug mode flag to print extra info
        
        List<Car> cars = new List<Car>(); // List to store Car objects
        
        // Path to the input CSV file 
        string relativePath = @"resources\SD-TA-001-B_DealershipStockList.csv";
        bool firstLine = true;

        using (var reader = new StreamReader(relativePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (firstLine) //Skip the header line in the CSV file
                {
                    firstLine = false;
                    continue;
                }

                var fields = line.Split(',');

                var car = new Car
                {
                    Registration = fields[0],
                    Make = fields[1],
                    Model = fields[2],
                    Mileage = int.Parse(fields[3]),
                    Price = int.Parse(fields[4])
                };

                cars.Add(car);
            }
        }



        // Calculate statistics using helper methods
        double avgPrice = CalculateAverage(cars, car => car.Price);    // Selector picks Price
        double avgMileage = CalculateAverage(cars, car => car.Mileage);// Selector picks Mileage
        Car cheapestCar = GetCheapestCar(cars);                        // Get cheapest
        (int totalCars, int totalValue) stockSummary = GetStockSummary(cars);  // Call once and store tuple result
        

        if (debug)
        {
            // Output total cars read into the list
            Console.WriteLine("The total amount of cars is: " + cars.Count());
            // Loop through list and print each car's details
            foreach (var car in cars)
            {
                Console.WriteLine($"Registration: {car.Registration} Make: {car.Make} Model: {car.Model} Mileage: {car.Mileage} Price: {car.Price}");
            }
            Console.WriteLine($"The average price: €{avgPrice:F2}");
            Console.WriteLine($"The average mileage: {avgMileage}km");
            Console.WriteLine($"Cheapest car is: {cheapestCar.Registration} {cheapestCar.Make} {cheapestCar.Model} at €{cheapestCar.Price}");
            Console.WriteLine($"The total amount of cars is: {stockSummary.totalCars}");
            Console.WriteLine($"The total value of all cars is: €{stockSummary.totalValue:F2}");
        }
        try
        {
            // Save statistics to an output file
            string filePath = "DealershipStockReport.txt";
            using (StreamWriter writer = new StreamWriter(filePath)) 
            {
                writer.WriteLine("Car Stock – Weekly Report");
                writer.WriteLine($"The average price: €{avgPrice:F2}");
                writer.WriteLine($"The average mileage: {avgMileage}km");
                writer.WriteLine($"Cheapest car is: {cheapestCar.Registration} {cheapestCar.Make} {cheapestCar.Model} at €{cheapestCar.Price}");
                writer.WriteLine($"The total amount of cars is: {stockSummary.totalCars}");
                writer.WriteLine($"The total value of all cars is: €{stockSummary.totalValue:F2}");
            }
        }
        catch (IOException e) {
            // If unable to write to file (due to IO exception) output error to the console
            Console.WriteLine("Unable to write to file due to:\n" + e.Message);
        }
    }
}
