import os

def migrate_files(src_dir, dest_dir, ns_replacements):
    for root, dirs, files in os.walk(src_dir):
        # Determinar caminho relativo
        rel_path = os.path.relpath(root, src_dir)
        target_dir = os.path.join(dest_dir, rel_path) if rel_path != '.' else dest_dir
        
        for file in files:
            if file.endswith('.cs'):
                src_file_path = os.path.join(root, file)
                dest_file_path = os.path.join(target_dir, file)
                
                os.makedirs(target_dir, exist_ok=True)
                
                with open(src_file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                # Fazer substituições de namespace
                for old_ns, new_ns in ns_replacements:
                    content = content.replace(old_ns, new_ns)
                
                with open(dest_file_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                
                print(f"Migrated and updated: {src_file_path} -> {dest_file_path}")

if __name__ == "__main__":
    # 1. Migração do Domain Comercial
    src_domain = "c:\\Projetos\\.NET\\ControlService\\src\\ControlService.Domain\\Commercial"
    dest_domain = "c:\\Projetos\\.NET\\ControlService\\src\\ControlService.Commercial\\ControlService.Commercial.Domain"
    domain_replacements = [
        ("ControlService.Domain.Commercial", "ControlService.Commercial.Domain"),
        ("namespace ControlService.Domain.Commercial", "namespace ControlService.Commercial.Domain")
    ]
    migrate_files(src_domain, dest_domain, domain_replacements)
    
    # 2. Migração do Application Comercial
    src_app = "c:\\Projetos\\.NET\\ControlService\\src\\ControlService.Application\\Commercial"
    dest_app = "c:\\Projetos\\.NET\\ControlService\\src\\ControlService.Commercial\\ControlService.Commercial.Application"
    app_replacements = [
        ("ControlService.Application.Commercial", "ControlService.Commercial.Application"),
        ("namespace ControlService.Application.Commercial", "namespace ControlService.Commercial.Application"),
        ("ControlService.Domain.Commercial", "ControlService.Commercial.Domain")
    ]
    migrate_files(src_app, dest_app, app_replacements)
