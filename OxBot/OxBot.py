from ox_svc import ListingsToExcel


# Create excel listing
listings_json_file_path = "E:/oxai/data/current/listings/listing_master_mirror.json"
excluded_fields = [
    'owconnect_category_id',
    'owconnect_category_name',
    'listing_id',
    'sequence',
    'logo: img/corp/generic-corp-logo-13.png'
]
# listings_to_excel = ListingsToExcel("E:/oxai/data/current/listings/listing_master_mirror.json", excluded_fields=excluded_fields)
# listings_to_excel.process_listings()