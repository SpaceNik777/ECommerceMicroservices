using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.ValueObjects
{
    public record Address
    {
        public string Street { get; init; }
        public string City { get; init; }
        public string State { get; init; }
        public string Country { get; init; }
        public string ZipCode { get; init; }

        public Address(string street, string city, string state, string country, string zipCode)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required");
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required");
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State is required");
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required");
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("ZipCode is required");

            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }

        protected virtual bool Validate()
        {
            return !string.IsNullOrEmpty(Street) &&
                   !string.IsNullOrEmpty(City) &&
                   !string.IsNullOrEmpty(State) &&
                   !string.IsNullOrEmpty(Country) &&
                   !string.IsNullOrEmpty(ZipCode);
        }
    }
}
