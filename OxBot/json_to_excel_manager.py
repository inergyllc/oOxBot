import json
import os
import pandas as pd

json_file_path = "assets/listing_master_test_mirror.json"
excel_file_path = "assets/listing_master_test_mirror.xlsx"

print(f"Converting {json_file_path} to {excel_file_path}...")

# Check if the file exists before attempting to remove it
if os.path.exists(excel_file_path):
    os.remove(excel_file_path)
    print(f"Removed {excel_file_path}...")

with open(json_file_path, 'r') as file:
    json_data = json.load(file)
print(f"Loaded {json_file_path}...")

# Normalize the top level of listings
listings_df = pd.json_normalize(json_data['listings'])
print("Normalized listings...")

# Initialize an empty DataFrame for all branches after flattening
all_branches_df = pd.DataFrame()

for listing in json_data['listings']:
    # Normalize branches for this listing if they exist
    if 'branches' in listing and listing['branches']:
        for branch in listing['branches']:
            # Normalize complex nested types within each branch
            branch_df = pd.json_normalize(branch, 
                                         record_path=['people'], 
                                         meta=['id', 'name', 'city', 'state', 
                                               ['geocode', 'summary'], 
                                               ['geocode', 'results']],
                                         errors='ignore')

            # Append each normalized branch to the all_branches_df DataFrame
            all_branches_df = all_branches_df.append(branch_df, ignore_index=True)

print("Normalized branches...")

# Now, you might want to combine listings_df with all_branches_df
# depending on how you want to structure your Excel file.
# For example, you might merge them on a common key or simply write them to separate sheets.

# Replace NaNs with empty strings or any other placeholder you prefer
listings_df.fillna("", inplace=True)
all_branches_df.fillna("", inplace=True)
print("Replaced NaNs with empty strings")

# Write DataFrames to an Excel file - you might want to use separate sheets or a single sheet
with pd.ExcelWriter(excel_file_path) as writer:
    listings_df.to_excel(writer, sheet_name='Listings', index=False)
    all_branches_df.to_excel(writer, sheet_name='Branches', index=False)
print(f"Converted to {excel_file_path}...")
