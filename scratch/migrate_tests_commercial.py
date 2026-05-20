import os

def replace_in_test_files(root_dir, replacements):
    for root, dirs, files in os.walk(root_dir):
        for file in files:
            if file.endswith('.cs'):
                file_path = os.path.join(root, file)
                
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                modified = False
                for old, new in replacements:
                    if old in content:
                        content = content.replace(old, new)
                        modified = True
                
                if modified:
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Updated namespaces in test: {file_path}")

if __name__ == "__main__":
    replacements = [
        ("ControlService.Domain.Commercial", "ControlService.Commercial.Domain"),
        ("ControlService.Application.Commercial", "ControlService.Commercial.Application")
    ]
    replace_in_test_files("c:\\Projetos\\.NET\\ControlService\\tests\\ControlService.Domain.Tests\\Commercial", replacements)
    replace_in_test_files("c:\\Projetos\\.NET\\ControlService\\tests\\ControlService.Application.Tests\\Commercial", replacements)
