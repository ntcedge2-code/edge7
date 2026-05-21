🏢 USER STORY 1 — INTERNAL (NTC PERSONNEL: DATA MIGRATION)
Title: Upload Existing Manual Applications to EDGE
As an NTC personnel,
I want to upload existing manual application records into EDGE via file upload, so that these records can be used by clients to speed up renewal processing.


Description:
NTC personnel will upload bulk manual application records (from legacy/manual processes) into EDGE using a CSV/Excel file. These records will populate the system and serve as the basis for renewal transactions.


Acceptance Criteria:
Access to Upload Module
NTC personnel can access an “Upload Existing Applications” page.
File Upload
System accepts:
CSV / Excel file formats
User can:
Select file
Click Upload
Data Validation
System validates:
Required fields (Reference No., License No., Name, etc.)
Correct format
Invalid records are flagged
Upload Result Summary
After upload, system displays:
Total records
Successfully uploaded
Failed records
Option to download error report
Data Storage
Successfully uploaded records are:
Stored in database
Tagged as “Migrated Data”
Audit Trail
System logs:
Uploaded by
Date/time
File name


🌐 USER STORY 2 — EXTERNAL (CLIENT: RENEWAL USING EXISTING DATA)
Title: Search and Use Existing Record for Renewal
As a client,
I want to search my existing application using my license or reference number,
So that I can renew without re-entering all my details.


Description:
Clients can search for their previously uploaded (migrated) application record. If found, the system pre-fills their data and allows them to proceed with renewal.


Acceptance Criteria:
Search Functionality
Client can input:
License Number OR
Reference Number
Click Search
Search Result
If record is found:
Display summary (Name, License No., Type)
Show button: “Proceed to Renewal”
If not found:
Display message:
“No record found. Please proceed with new application.”
Auto-Fill Form
Upon proceeding:
System pre-fills application form with retrieved data
Editable Fields
Client can update necessary fields (if allowed)
Renewal Submission
Client submits renewal without re-entering all data
Data Indicator
System shows label:
“Data retrieved from previous record”


🎨 UI BEHAVIOR SUMMARY (FOR DEVELOPERS)
Internal (NTC)
Menu: Upload Existing Applications
Components:
File Upload Button
Upload Summary Table
Error Report Download


External (Client)
Step 1: Search
Input field: License / Reference Number
Button: Search
Step 2: Result
If found → Show summary + Proceed to Renewal
If not → Redirect to New Application
Step 3: Renewal Form
Pre-filled fields
Editable where allowed
Submit button


💬 Optional Note (for your documentation)
This feature serves as a one-time migration solution to support the transition from manual processing to a fully digital EDGE system. Once all records are uploaded, all future transactions will be managed entirely within EDGE.