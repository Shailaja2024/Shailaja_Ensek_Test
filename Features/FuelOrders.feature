Feature: FuelOrders

@Api_01
Scenario: 01_Reset the test data
	Given User is authorised
	When User 'reset' the test data
	Then the data should be cleared and reset to default values

@Api_02
Scenario Outline: 02_Buy a Quantity of Each Fuel
	Given User is authorised
	When User purchase a quantity of fuel type '<Fuel ID>' with '<Quantity To Purchase>' using 'buy' endpoint
	Then User should recieve a confirmation for purchase
	And Verify each order should be present in the response from the 'orders' endpoint
Examples:
	| Fuel ID | Quantity To Purchase |
	| 1       | 5                    |
	| 2       | 10                   |
	| 3       | 15                   |
	| 4       | 20                   |

@Api_03
Scenario: 03_Confirm How Many Orders Were Created Before the Current Date
	Given User have a list of all orders from the 'orders' endpoint
	When User filter orders that have a purchase timestamp before the current date
	Then the count should match the expected number of past orders

Scenario: 04_Validate Invalid Response Status Codes
Given User is authorised
When User send a request with an invalid 1500 with 1 using 'buy' endpoint
Then the status code should be 400