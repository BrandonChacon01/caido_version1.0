import json
import matplotlib.pyplot as plt
import matplotlib
import os
from pathlib import Path

# Usar backend sin GUI para GitHub Actions
matplotlib.use('Agg')

def generate_charts():
    """Genera gráficas PNG de los metrics"""
    
    if not os.path.exists("metrics/assets_report.json"):
        print("⚠️ No se encontró assets_report.json")
        return
    
    with open("metrics/assets_report.json", "r", encoding="utf-8") as f:
        report = json.load(f)
    
    metrics = report.get("metrics", {})
    total_size_mb = report.get("total_size_mb", 0)
    
    print("\n" + "="*60)
    print("🎨 GENERANDO GRÁFICAS")
    print("="*60)
    
    # Colores personalizados
    colors = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F7DC6F', '#BB8FCE', '#A8E6CF']
    
    # ========== GRÁFICA 1: Pastel - Distribución por Tipo ==========
    print("📊 Generando gráfica de distribución (pastel)...")
    fig, ax = plt.subplots(figsize=(12, 8))
    
    types = []
    sizes = []
    explode = []
    
    for i, (asset_type, data) in enumerate(metrics.items()):
        if data['count'] > 0:
            types.append(f"{asset_type.upper()}\n{data['count']} archivos")
            sizes.append(data['total_size_mb'])
            # Destacar el más grande
            explode.append(0.1 if data['total_size_mb'] == max(d['total_size_mb'] for d in metrics.values() if d['count'] > 0) else 0)
    
    wedges, texts, autotexts = ax.pie(
        sizes, 
        labels=types, 
        autopct='%1.1f%%',
        colors=colors[:len(types)],
        startangle=90,
        explode=explode,
        shadow=True
    )
    
    # Mejorar el texto
    for autotext in autotexts:
        autotext.set_color('white')
        autotext.set_fontweight('bold')
        autotext.set_fontsize(10)
    
    for text in texts:
        text.set_fontsize(10)
        text.set_fontweight('bold')
    
    ax.set_title(f'Distribución de Tamaño por Tipo de Asset\nTotal: {total_size_mb:.2f} MB', 
                fontsize=14, fontweight='bold', pad=20)
    
    plt.tight_layout()
    plt.savefig('metrics/01_distribution_pie_chart.png', dpi=150, bbox_inches='tight', facecolor='white')
    plt.close()
    print("✅ Gráfica de distribución guardada")
    
    # ========== GRÁFICA 2: Barras - Comparativa por Tipo ==========
    print("📊 Generando gráfica comparativa (barras)...")
    fig, ax = plt.subplots(figsize=(12, 6))
    
    types_list = []
    sizes_list = []
    counts_list = []
    
    for asset_type, data in sorted(metrics.items(), key=lambda x: x[1]['total_size_mb'], reverse=True):
        if data['count'] > 0:
            types_list.append(asset_type.upper())
            sizes_list.append(data['total_size_mb'])
            counts_list.append(data['count'])
    
    x = range(len(types_list))
    bars = ax.bar(x, sizes_list, color=colors[:len(types_list)], edgecolor='black', linewidth=1.5)
    
    ax.set_xticks(x)
    ax.set_xticklabels(types_list, rotation=45, ha='right', fontsize=11, fontweight='bold')
    ax.set_ylabel('Tamaño (MB)', fontweight='bold', fontsize=12)
    ax.set_title('Tamaño Total por Tipo de Asset', fontsize=14, fontweight='bold', pad=20)
    ax.grid(axis='y', alpha=0.3, linestyle='--')
    
    # Agregar valores en las barras
    for bar, size, count in zip(bars, sizes_list, counts_list):
        height = bar.get_height()
        ax.text(bar.get_x() + bar.get_width()/2., height,
               f'{size:.1f} MB\n({count} archivos)',
               ha='center', va='bottom', fontsize=9, fontweight='bold')
    
    plt.tight_layout()
    plt.savefig('metrics/02_types_comparison_bar_chart.png', dpi=150, bbox_inches='tight', facecolor='white')
    plt.close()
    print("✅ Gráfica comparativa guardada")
    
    # ========== GRÁFICA 3: Barras Horizontal - Top 20 Assets ==========
    print("📊 Generando gráfica de top assets...")
    
    all_assets = []
    for asset_type, data in metrics.items():
        for asset in data.get('top_10', []):
            all_assets.append({
                'name': asset['name'][:40],  # Limitar nombre
                'size_mb': asset['size_mb'],
                'type': asset_type
            })
    
    if all_assets:
        all_assets.sort(key=lambda x: x['size_mb'], reverse=True)
        top_20 = all_assets[:20]
        
        fig, ax = plt.subplots(figsize=(14, 10))
        
        names = [a['name'] for a in top_20]
        sizes = [a['size_mb'] for a in top_20]
        types = [a['type'] for a in top_20]
        
        # Mapeo de colores por tipo
        type_colors = {
            'prefabs': '#FF6B6B',
            'scenes': '#4ECDC4',
            'meshes': '#45B7D1',
            'textures': '#FFA07A',
            'materials': '#98D8C8',
            'audio': '#F7DC6F',
            'scripts': '#BB8FCE',
            'otros': '#999999'
        }
        
        colors_list = [type_colors.get(t, '#999999') for t in types]
        
        bars = ax.barh(range(len(names)), sizes, color=colors_list, edgecolor='black', linewidth=0.5)
        ax.set_yticks(range(len(names)))
        ax.set_yticklabels(names, fontsize=9)
        ax.set_xlabel('Tamaño (MB)', fontweight='bold', fontsize=12)
        ax.set_title('Top 20 Assets más Pesados', fontsize=14, fontweight='bold', pad=20)
        ax.invert_yaxis()
        ax.grid(axis='x', alpha=0.3, linestyle='--')
        
        # Agregar valores en las barras
        for i, (bar, size) in enumerate(zip(bars, sizes)):
            ax.text(size, bar.get_y() + bar.get_height()/2, f' {size:.2f} MB', 
                   va='center', fontsize=8, fontweight='bold')
        
        plt.tight_layout()
        plt.savefig('metrics/03_top_assets_bar_chart.png', dpi=150, bbox_inches='tight', facecolor='white')
        plt.close()
        print("✅ Gráfica de top assets guardada")
    
    print("="*60 + "\n")

if __name__ == "__main__":
    generate_charts()