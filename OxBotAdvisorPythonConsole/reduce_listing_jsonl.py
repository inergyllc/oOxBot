import json
import os
import re

def reduce_listing_jsonl(source_file, target_file):
    # Get the size of the source file
    source_file_size = os.path.getsize(source_file)
    print(f"Opening file: {source_file} (Size: {source_file_size:,} bytes)")
    remove_merge_string_1 = "|contact=No Contact|contact title=|contact email=No Contact|contact phone=|"
    remove_merge_string_2 = "|contact title=|contact email=No contact|contact phone=|"
    remove_merge_string_3 = "|contact email=No contact|contact phone=|"
    remove_merge_string_4 = "|contact email=No contact"
    generic_logo_prefix = "img/corp/generic-corp"
    remove_unknown_url = "unknown-company-website.html"    
    ctr = 0
    with open(source_file, 'r', encoding='latin-1') as src, open(target_file, 'w', encoding='latin-1') as tgt:
        for line in src:
            ctr += 1
            data = json.loads(line)

            # Function to format person data into a string
            def format_person(person):
                person_details = [
                    person.get("name", ""),
                    person.get("title", ""),
                    person.get("email", ""),
                    person.get("phone", "")
                ]
                # Filter out empty details and join them with a comma
                return ', '.join(filter(None, person_details))

            def convert_to_hq(field):
                if field and "headquarters" in field.lower():
                    return "hq"
                return field

            # Check if the mailing and physical addresses are the same
            mail_addr = data.get("raw_mailing_address")
            real_addr = data.get("raw_physical_address")
            if mail_addr != real_addr:
                address_info = {
                    "mail_addr": mail_addr,
                    "real_addr": real_addr
                }
            else:
                address_info = {"real_addr": real_addr}
            
            merged_field = data.get("merged", "") + "|"
            merged_field = merged_field.replace(remove_merge_string_1, "")
            merged_field = merged_field.replace(remove_merge_string_2, "")
            merged_field = merged_field.replace(remove_merge_string_3, "")
            merged_field = merged_field.replace(remove_merge_string_4, "")

            subscription_level_field = data.get("subscription_level", "")
            subscription_level_field = subscription_level_field.replace("guest", "g")
            subscription_level_field = subscription_level_field.replace("personal+", "p+")
            subscription_level_field = subscription_level_field.replace("personal", "p")
            subscription_level_field = subscription_level_field.replace("corporate 1 user", "c1")
            subscription_level_field = subscription_level_field.replace("corporate 50 user", "c50")
            subscription_level_field = subscription_level_field.replace("corporate 5 user", "c5")
            subscription_level_field = subscription_level_field.replace("corporate enterprise", "cent")
            
            logo_field = data.get("logo", "")
            if logo_field.startswith(generic_logo_prefix):
                logo_field = None
                
            website_url_field = data.get("website_url", "")
            if website_url_field.startswith(remove_unknown_url):
                website_url_field = None
            # Trim the data with conditions
            trimmed_data = {key: value for key, value in {
                "id": data.get("id"),
                "bid": data.get("branch_id"),
                "bname": convert_to_hq(data.get("branch_name")),
                "email": data.get("email"),
                "fon1": data.get("phone_1"),
                "fon2": data.get("phone_2"),
                **address_info,
                "zip": data.get("zip"),
                # "is_hq": data.get("is_hq"),
                "btype": convert_to_hq(data.get("branch_type")),
                "people": [format_person(person) for person in data.get("people", []) if person] if data.get("people") else None,
                "fon": data.get("phone"),
                "category": data.get("category"),
                "name": data.get("name"),
                "url": website_url_field,
                "logo": logo_field,
                "cat_id": data.get("category_id"),
                "stars": data.get("star_rating"),
                "featured": data.get("is_featured"),
                # "slogan": data.get("slogan"),
                "why_featured": data.get("why_featured", []),
                "merged": merged_field,
                "sub": subscription_level_field,
                "lid": data.get("listing_id")
            }.items() if value not in [None, '', [], {}]}

            # Write the trimmed data to the target file
            json.dump(trimmed_data, tgt)
            tgt.write('\n')

    # Print completion message
    target_file_size = os.path.getsize(target_file)
    print(f"Completed file: {target_file} (Size: {target_file_size:,} bytes, Rows: {ctr})")

# Example usage
# source_file_path = 'E:\\oxai\\data\\current\\listings\\geo_flat_listing_jsonl.json'
# target_file_path = 'E:\\oxai\\data\\current\\listings\\geo_flat_listing_min_jsonl.json'
# reduce_listing_jsonl(source_file_path, target_file_path)
