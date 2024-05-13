using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CountryServices
{
    /// <summary>
    /// Provides information about country local currency from RESTful API.
    /// <see><cref>https://restcountries.com/#api-endpoints-v2</cref></see>.
    /// </summary>
    public class CountryService : ICountryService
    {
        private readonly string serviceUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryService"/> class with specified <see cref="serviceUrl"/>.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        public CountryService(string serviceUrl)
        {
            this.serviceUrl = serviceUrl;
        }

        /// <summary>
        /// Gets information about currency by country code synchronously.
        /// </summary>
        /// <param name="alpha2Or3Code">ISO 3166-1 2-letter or 3-letter country code.</param>
        /// <see>
        ///     <cref>https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes</cref>
        /// </see>
        /// <returns>Information about country currency as <see cref="LocalCurrency"/>>.</returns>
        /// <exception cref="ArgumentException">Throw if countryCode is null, empty, whitespace or invalid country code.</exception>
        public LocalCurrency GetLocalCurrencyByAlpha2Or3Code(string? alpha2Or3Code)
        {
            ValidateCountryCode(alpha2Or3Code!);

            try
            {
                using var webClient = new WebClient();
                var url = $"{this.serviceUrl}/alpha/{alpha2Or3Code}";
                var json = webClient.DownloadString(url);
                var currencyInfo = JsonSerializer.Deserialize<JsonElement>(json);

                if (currencyInfo.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("No valid currency information found in the JSON response.");
                }

                var countryName = currencyInfo.GetProperty("name").GetString() ?? string.Empty;

                var currenciesArray = currencyInfo.GetProperty("currencies");

                if (currenciesArray.ValueKind != JsonValueKind.Array || currenciesArray.GetArrayLength() == 0)
                {
                    throw new JsonException("No valid currency information found in the JSON response.");
                }

                var firstCurrency = currenciesArray.EnumerateArray().First();
                string? currencyCode = firstCurrency.GetProperty("code").GetString();
                string? currencySymbol = firstCurrency.GetProperty("symbol").GetString();

                return new LocalCurrency
                {
                    CountryName = countryName,
                    CurrencyCode = currencyCode,
                    CurrencySymbol = currencySymbol,
                };
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException("Failed to retrieve currency info. Check your network connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to parse JSON response.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving currency info.", ex);
            }
        }

        /// <summary>
        /// Gets information about currency by country code asynchronously.
        /// </summary>
        /// <param name="alpha2Or3Code">ISO 3166-1 2-letter or 3-letter country code.</param>
        /// <see>
        ///     <cref>https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes</cref>
        /// </see>
        /// <param name="token">Token for cancellation asynchronous operation.</param>
        /// <returns>Information about country currency as <see cref="LocalCurrency"/>>.</returns>
        /// <exception cref="ArgumentException">Throw if countryCode is null, empty, whitespace or invalid country code.</exception>
        public async Task<LocalCurrency> GetLocalCurrencyByAlpha2Or3CodeAsync(string? alpha2Or3Code, CancellationToken token)
        {
            ValidateCountryCode(alpha2Or3Code!);

            try
            {
                using var httpClient = new HttpClient();
                var url = $"{this.serviceUrl}/alpha/{alpha2Or3Code}";
                var json = await httpClient.GetStringAsync(new Uri(url), token);
                var currencyInfo = JsonSerializer.Deserialize<JsonElement>(json);

                if (currencyInfo.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("No valid currency information found in the JSON response.");
                }

                var countryName = currencyInfo.GetProperty("name").GetString() ?? string.Empty;

                var currenciesArray = currencyInfo.GetProperty("currencies");

                if (currenciesArray.ValueKind != JsonValueKind.Array || currenciesArray.GetArrayLength() == 0)
                {
                    throw new JsonException("No valid currency information found in the JSON response.");
                }

                var firstCurrency = currenciesArray.EnumerateArray().First();

                string? currencyCode = firstCurrency.GetProperty("code").GetString();
                string? currencySymbol = firstCurrency.GetProperty("symbol").GetString();

                return new LocalCurrency
                {
                    CountryName = countryName,
                    CurrencyCode = currencyCode,
                    CurrencySymbol = currencySymbol,
                };
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to retrieve currency info. Check your network connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to parse JSON response.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving currency info.", ex);
            }
        }

        /// <summary>
        /// Gets information about the country by the country capital synchronously.
        /// </summary>
        /// <param name="capital">Capital name.</param>
        /// <returns>Information about the country as <see cref="CountryInfo"/>>.</returns>
        /// <exception cref="ArgumentException">Throw if the capital name is null, empty, whitespace or nonexistent.</exception>
        public CountryInfo GetCountryInfoByCapital(string? capital)
        {
            ValidateCapitalName(capital!);

            try
            {
                using var webClient = new WebClient();
                var url = $"{this.serviceUrl}/capital/{capital}";
                var json = webClient.DownloadString(url);
                var countryInfoArray = JsonSerializer.Deserialize<JsonElement[]>(json);

                if (countryInfoArray == null || countryInfoArray.Length == 0)
                {
                    throw new JsonException("No valid country information found in the JSON response.");
                }

                var countryInfoElement = countryInfoArray[0];
                var name = countryInfoElement.GetProperty("name").GetString() ?? string.Empty;
                var capitalName = countryInfoElement.GetProperty("capital").GetString() ?? string.Empty;
                double area = countryInfoElement.GetProperty("area").GetDouble();
                int population = countryInfoElement.GetProperty("population").GetInt32();
                var flag = countryInfoElement.GetProperty("flag").GetString() ?? string.Empty;

                return new CountryInfo
                {
                    Name = name,
                    CapitalName = capitalName,
                    Area = area,
                    Population = population,
                    Flag = flag,
                };
            }
            catch (WebException ex)
            {
                throw new ArgumentException("Invalid capital name.", nameof(capital), ex);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to parse JSON response.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving currency info.", ex);
            }
        }

        /// <summary>
        /// Gets information about the currency by the country capital asynchronously.
        /// </summary>
        /// <param name="capital">Capital name.</param>
        /// <param name="token">Token for cancellation asynchronous operation.</param>
        /// <returns>Information about the country as <see cref="CountryInfo"/>>.</returns>
        /// <exception cref="ArgumentException">Throw if the capital name is null, empty, whitespace or nonexistent.</exception>
        public async Task<CountryInfo> GetCountryInfoByCapitalAsync(string? capital, CancellationToken token)
        {
            ValidateCapitalName(capital!);

            try
            {
                using var httpClient = new HttpClient();
                var url = $"{this.serviceUrl}/capital/{capital}";
                var json = await httpClient.GetStringAsync(new Uri(url), token);
                var countryInfoArray = JsonSerializer.Deserialize<JsonElement[]>(json);

                if (countryInfoArray == null || countryInfoArray.Length == 0)
                {
                    throw new JsonException("No valid country information found in the JSON response.");
                }

                var countryInfoElement = countryInfoArray[0];
                var name = countryInfoElement.GetProperty("name").GetString() ?? string.Empty;
                var capitalName = countryInfoElement.GetProperty("capital").GetString() ?? string.Empty;
                double area = countryInfoElement.GetProperty("area").GetDouble();
                int population = countryInfoElement.GetProperty("population").GetInt32();
                var flag = countryInfoElement.GetProperty("flag").GetString() ?? string.Empty;

                return new CountryInfo
                {
                    Name = name,
                    CapitalName = capitalName,
                    Area = area,
                    Population = population,
                    Flag = flag,
                };
            }
            catch (HttpRequestException ex)
            {
                throw new ArgumentException("Invalid capital name.", nameof(capital), ex);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to parse JSON response.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving country info.", ex);
            }
        }

        /// <summary>
        /// Validates the country code.
        /// </summary>
        /// <param name="code">The country code to validate.</param>
        private static void ValidateCountryCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2 || code.Length > 3)
            {
                throw new ArgumentException("Country code must be a valid ISO 3166-1 2-letter or 3-letter code.", nameof(code));
            }
        }

        /// <summary>
        /// Validates the capital name.
        /// </summary>
        /// <param name="capital">The capital name to validate.</param>
        private static void ValidateCapitalName(string capital)
        {
            if (string.IsNullOrWhiteSpace(capital))
            {
                throw new ArgumentException("Capital name cannot be null or whitespace.", nameof(capital));
            }
        }
    }
}
