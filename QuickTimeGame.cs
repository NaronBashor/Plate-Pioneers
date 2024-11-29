def decode_secret_message(input_data):
    entries = []
    for line in input_data.strip().split("\n"):
        parts = line.split()
        x = int(parts[0])  # x-coordinate
        char = parts[1]    # Character
        y = int(parts[2])  # y-coordinate
        entries.append((x, y, char))
    
    max_x = max(entry[0] for entry in entries)
    max_y = max(entry[1] for entry in entries)
    
    grid = [[" " for _ in range(max_x + 1)] for _ in range(max_y + 1)]
    
    for x, y, char in entries:
        grid[y][x] = char
    
    for row in grid:
        print("".join(row))

input_data = """
0 ▮ 0
0 ▮ 1
0 ▮ 2
1 ▮ 0
1 ▮ 1
2 ▮ 0
"""

decode_secret_message(input_data)
