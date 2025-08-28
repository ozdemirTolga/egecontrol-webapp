-- Update all quotes without currency to EUR as default
UPDATE Quotes 
SET Currency = 'EUR' 
WHERE Currency IS NULL OR Currency = '';

-- Check the results
SELECT Id, QuoteNumber, Title, Currency, TotalAmount FROM Quotes;
