import requests

def decode_secret_message_from_url(url):
    # Step 1: Fetch the content from the Google Docs URL
    response = requests.get(url)
    
    if response.status_code != 200:
        print("Failed to fetch the document. Please check the URL.")
        return
    
    # Step 2: Parse the content
    input_data = response.text.strip()
    
    # Step 3: Decode the message using the existing function logic
    decode_secret_message(input_data)

def decode_secret_message(input_data):
    # Parse the input into a list of coordinates and characters
    entries = []
    for line in input_data.strip().split("\n"):
        parts = line.split()
        x = int(parts[0])  # x-coordinate
        char = parts[1]    # Character
        y = int(parts[2])  # y-coordinate
        entries.append((x, y, char))
    
    # Determine the dimensions of the grid
    max_x = max(entry[0] for entry in entries)
    max_y = max(entry[1] for entry in entries)
    
    # Create an empty grid filled with spaces
    grid = [[" " for _ in range(max_x + 1)] for _ in range(max_y + 1)]
    
    # Place characters in the grid based on coordinates
    for x, y, char in entries:
        grid[y][x] = char
    
    # Print the grid row by row
    for row in grid:
        print("".join(row))

# Test the function with the Google Docs URL
decode_secret_message_from_url("https://docs.google.com/document/d/e/2PACX-1vQGUck9HIFCyezsrBSnmENk5ieJuYwpt7YHYEzeNJkIb9OSDdx-ov2nRNReKQyey-cwJOoEKUhLmN9z/pub")
