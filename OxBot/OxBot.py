from ox_svc import ListingsToExcel

# Create excel listing
listings_json_file_path = "E:/oxai/data/current/listings/listing_master_mirror.json"
excluded_fields = [
    {'type': 'listing', 'field': 'owconnect_category_id'},
    {'type': 'listing', 'field': 'owconnect_category_name'},
    {'type': 'listing', 'field': 'listing_id'},
    {'type': 'listing', 'field': 'sequence'},
    {'type': 'listing', 'field': 'branches'},
    {'type': 'listing', 'field': 'hq_pos'},
    {'type': 'listing', 'field': 'hq_lat'},
    {'type': 'listing', 'field': 'hq_lon'},
    {'type': 'listing', 'field': 'city_st'},
    {'type': 'listing', 'field': 'merged'},
    {'type': 'branch', 'field': 'listing_id'},
    {'type': 'branch', 'field': 'geocode'},
    {'type': 'branch', 'field': 'people'},
    {'type': 'people', 'field': 'id'},
    {'type': 'people', 'field': 'branch_id'}
]
xlsx_file_name = None  # If None, will be generated from the json file name
rows_to_flatten=3 # If None, will process all listings
listings_to_excel = ListingsToExcel(
   filename=listings_json_file_path,
   excluded_fields=excluded_fields,
   output_xlsx=xlsx_file_name,
   max_rows_flattened=rows_to_flatten)
listings_to_excel.process_listings()
print(listings_to_excel.output_xlsx)