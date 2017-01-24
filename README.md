<h1> Application for QuickBooks Online
<h4>The application write sales tax into estimates, invoices, sales receipts, credit memos. Sales tax amount depends on customer shipTo state (5% for California, 6% for New York, 7% for other ship to addresses). Document total sales tax amount calculated as multiplication document total amount on tax rate. Invoices, sales receipts and credit memos persists into “report” table. Invoices persists depends on Accounting Method(Accrual Basis or Cash Basis). Application don't store deleted documents and contain only most recent documents data. Also all documents are recalculated in Quickbooks Online.</h4>
<h2>First Use Instructions:</h2>
<ul>
<li>Clone the GitHub repo to your workspace.</li>
<li>Set appropriate values into the project's web.config file(list of values see below).</li>
<li>Set Tax rates into database depends on state.</li>
<li> Run the application.</li>
<li>Click on the buttons in the following order:
<ul>
1. Connect your app to Quickbooks, by clicking on Connect to QuickBooks button and follow the instructions on the screen.<br>
2. Recalculate button.<br>
3. Save button.
</ul></li>
</ul>

<h2>To get started the application set the next values into the project's web.config file:</h2>
1) Consumer key and consumer secret;<br>
2) AppToken;<br>
3) WebHooksVerifier and WebhooksEntities;<br>
4) RealmId;<br>
5) OauthCallbackUrl(instead of localhost:63793 write your domain).<br>
