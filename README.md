# unity-iap
A quick implementation of Unity IAP service.

# Requirements
1. Enabled Unity IAP services.
2. "In App Purchasing" package must be version 4.5.1 or higher.
3. Have a Play Console developer account if building for Android and Play Store Connect developer account if iOS.

# How To Use
1. Import package into your project.
2. Setup license key. (optional, only for targetting Android)
- Go to https://play.google.com/apps/publish/ and choose your project.
- Go to Monetizing/Monetization setup and get Google license key.
- Go to https://dashboard.unity3d.com/ , choose your project, select Analytics/Analytics setting and then paste the Google license key.
- In Unity, go to Services/In-App Purchasing/Receipt Validation Obfuscator and paste the Google license key, and obfuscate it.
3. Setup the product catalog
- In Unity, go to Services/In-App Purchasing/IAP Catalog
- Enter your iap product informations.
- (Only for Play Console) Export to CSV and go to 
4. Setup Play Console (optional)
- If you have trouble importing IAP Catalog CSV using USD. You can setup pricing template as a workaround.
- Go to Play Console/Princing Template and setup your templates. (Note the template ID, you'll need that later)
- Back to Unity IAP Catalog, change product Google Configuration to Pricing, clear the fixed Price and paste the template ID.
- Export the catalog to CSV and import to Play Console again.
5. Setup App Store Connect (optional)
- App Store stopped supporting importing from IAP Catalog, so you have to manual create the product one by one on the store.
6. Usage:
- Register to OnInitialized and call Initialize() at the beginning.
- In your code, call "IAPManager.Instance.BuyProduct(string productId)" to buy the product with id "productId" (which is listed in product catalog).
- Register "OnPurchase" event to get notifies about the purchase.
- Register "OnRestore" event to get notification about the restore request.
- Call "...RestoreProducts()" to restore users' purchased non-consumable products. (The events will go to OnPurchase).
- Call "GetProductPriceById(string productId)" to get the price. (E.g $10)
- Call "GetProductValueById(string productId)" to get the value. (E.g 10k Coins)
- Call "GetProductById(string productId)" to get the product information. (Included these two from above)

# Note
- Call "Initialize()" to re-initialize the service if starting the game without internet connection. (Register to "OnInitialized" to know what-is-going-on to the service).

# Troubleshooting
- Google Rejection's Error: "We've detected this app uses an unsupported version of Play billing. Please upgrade to Billing Library version 4 or newer to publish this app."
=> Please update the Unity IAP package to 4.4.1 or newer.

------
It's not that quick, eh?
Happy developing.
