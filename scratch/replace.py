import os

def replace_in_file(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    modified = False
    if "ControlService.Domain.SeedWork" in content:
        content = content.replace("ControlService.Domain.SeedWork", "ControlService.SharedKernel.SeedWork")
        modified = True
    
    if "ControlService.Application.Behaviors" in content:
        content = content.replace("ControlService.Application.Behaviors", "ControlService.SharedKernel.Behaviors")
        modified = True
        
    if modified:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Updated: {file_path}")

def walk_and_replace(root_dir):
    for root, dirs, files in os.walk(root_dir):
        if 'bin' in dirs:
            dirs.remove('bin')
        if 'obj' in dirs:
            dirs.remove('obj')
        if '.vs' in dirs:
            dirs.remove('.vs')
        if '.git' in dirs:
            dirs.remove('.git')
            
        for file in files:
            if file.endswith('.cs'):
                replace_in_file(os.path.join(root, file))

if __name__ == "__main__":
    walk_and_replace("c:\\Projetos\\.NET\\ControlService\\src")
    walk_and_replace("c:\\Projetos\\.NET\\ControlService\\tests")
