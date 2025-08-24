#!/usr/bin/env python3
"""
Comprehensive Test Coverage Analysis Tool
Analyzes Swift vs C# test coverage for NeoUnity project
"""

import os
import re
from pathlib import Path
from collections import defaultdict, Counter
from typing import Dict, List, Set, Tuple

class TestCoverageAnalyzer:
    def __init__(self):
        self.swift_tests = defaultdict(list)  # {filename: [test_methods]}
        self.csharp_tests = defaultdict(list)  # {filename: [test_methods]}
        self.categories = {
            'contract': 'Contract Tests',
            'crypto': 'Crypto Tests', 
            'protocol': 'Protocol Tests',
            'script': 'Script Tests',
            'serialization': 'Serialization Tests',
            'transaction': 'Transaction Tests',
            'types': 'Type Tests',
            'wallet': 'Wallet Tests',
            'witnessrule': 'Witness Rule Tests',
            'helpers': 'Helper/Mock Files',
            'core': 'Core Tests'
        }
    
    def extract_test_methods(self, file_path: str, is_swift: bool) -> List[str]:
        """Extract test method names from a test file"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            if is_swift:
                # Swift test methods: public func test... or func test...
                pattern = r'(?:public\s+)?func\s+(test\w+)\s*\([^)]*\)'
            else:
                # C# test methods: [Test] or [UnityTest] followed by method
                pattern = r'\[(?:Test|UnityTest)[^\]]*\]\s*(?:public\s+)?(?:async\s+)?(?:Task\s+|void\s+|IEnumerator\s+)(\w*Test\w*)'
            
            methods = re.findall(pattern, content, re.MULTILINE)
            return [m for m in methods if m.lower().startswith('test')]
            
        except Exception as e:
            print(f"Error processing {file_path}: {e}")
            return []
    
    def analyze_swift_tests(self, swift_test_dir: str):
        """Analyze all Swift test files"""
        swift_path = Path(swift_test_dir)
        
        for test_file in swift_path.rglob('*.swift'):
            # Skip helper files
            if any(helper in test_file.name.lower() for helper in ['json', 'mock', 'testproperties']):
                continue
                
            relative_path = test_file.relative_to(swift_path)
            test_methods = self.extract_test_methods(str(test_file), is_swift=True)
            
            if test_methods:
                self.swift_tests[str(relative_path)] = test_methods
    
    def analyze_csharp_tests(self, csharp_test_dir: str):
        """Analyze all C# test files"""
        csharp_path = Path(csharp_test_dir)
        
        for test_file in csharp_path.rglob('*.cs'):
            # Skip helper files
            if any(helper in test_file.name.lower() for helper in ['mock', 'helper', 'testsuiterunner']):
                continue
                
            relative_path = test_file.relative_to(csharp_path)
            test_methods = self.extract_test_methods(str(test_file), is_swift=False)
            
            if test_methods:
                self.csharp_tests[str(relative_path)] = test_methods
    
    def categorize_tests(self, test_dict: Dict) -> Dict[str, Dict]:
        """Categorize tests by domain"""
        categorized = defaultdict(lambda: defaultdict(list))
        
        for file_path, methods in test_dict.items():
            # Determine category from path
            category = 'other'
            for cat_key in self.categories:
                if cat_key in file_path.lower():
                    category = cat_key
                    break
            
            categorized[category][file_path] = methods
        
        return categorized
    
    def find_missing_coverage(self) -> Dict:
        """Identify missing test coverage areas"""
        swift_categorized = self.categorize_tests(self.swift_tests)
        csharp_categorized = self.categorize_tests(self.csharp_tests)
        
        missing_coverage = {
            'missing_files': [],
            'missing_categories': [],
            'partial_coverage': [],
            'c_sharp_enhancements': []
        }
        
        # Check for missing files/categories
        for category, swift_files in swift_categorized.items():
            if category not in csharp_categorized:
                missing_coverage['missing_categories'].append({
                    'category': category,
                    'files': list(swift_files.keys()),
                    'test_count': sum(len(methods) for methods in swift_files.values())
                })
            else:
                # Check for missing files within category
                swift_file_names = set(Path(f).stem.replace('Tests', '') for f in swift_files.keys())
                csharp_file_names = set(Path(f).stem.replace('Tests', '') for f in csharp_categorized[category].keys())
                
                missing_files = swift_file_names - csharp_file_names
                for missing_file in missing_files:
                    # Find the original swift file
                    for swift_file, methods in swift_files.items():
                        if Path(swift_file).stem.replace('Tests', '') == missing_file:
                            missing_coverage['missing_files'].append({
                                'category': category,
                                'file': swift_file,
                                'missing_methods': len(methods),
                                'methods': methods
                            })
        
        # Check C# enhancements (extra tests not in Swift)
        for category, csharp_files in csharp_categorized.items():
            if category in swift_categorized:
                csharp_total = sum(len(methods) for methods in csharp_files.values())
                swift_total = sum(len(methods) for methods in swift_categorized[category].values())
                
                if csharp_total > swift_total:
                    missing_coverage['c_sharp_enhancements'].append({
                        'category': category,
                        'swift_tests': swift_total,
                        'csharp_tests': csharp_total,
                        'enhancement_factor': round(csharp_total / swift_total if swift_total > 0 else float('inf'), 2)
                    })
        
        return missing_coverage
    
    def generate_comprehensive_report(self) -> str:
        """Generate comprehensive test coverage analysis report"""
        swift_categorized = self.categorize_tests(self.swift_tests)
        csharp_categorized = self.categorize_tests(self.csharp_tests)
        missing_coverage = self.find_missing_coverage()
        
        report = []
        report.append("# 🧪 ULTIMATE TEST COMPLETENESS VERIFICATION REPORT")
        report.append("## NeoUnity: Swift → C# Test Coverage Analysis")
        report.append("")
        
        # Summary statistics
        swift_total_files = len(self.swift_tests)
        swift_total_methods = sum(len(methods) for methods in self.swift_tests.values())
        csharp_total_files = len(self.csharp_tests)  
        csharp_total_methods = sum(len(methods) for methods in self.csharp_tests.values())
        
        report.append("## 📊 EXECUTIVE SUMMARY")
        report.append(f"- **Swift Test Files**: {swift_total_files}")
        report.append(f"- **Swift Test Methods**: {swift_total_methods}")
        report.append(f"- **C# Test Files**: {csharp_total_files}")
        report.append(f"- **C# Test Methods**: {csharp_total_methods}")
        report.append(f"- **Coverage Enhancement Factor**: {round(csharp_total_methods/swift_total_methods if swift_total_methods > 0 else 0, 2)}x")
        report.append("")
        
        # Category-by-category analysis
        report.append("## 🎯 DETAILED COVERAGE ANALYSIS BY CATEGORY")
        report.append("")
        
        all_categories = set(swift_categorized.keys()) | set(csharp_categorized.keys())
        
        for category in sorted(all_categories):
            category_name = self.categories.get(category, category.title())
            swift_files = swift_categorized.get(category, {})
            csharp_files = csharp_categorized.get(category, {})
            
            swift_count = sum(len(methods) for methods in swift_files.values())
            csharp_count = sum(len(methods) for methods in csharp_files.values())
            
            status = "✅ COMPLETE" if csharp_count >= swift_count and csharp_files else "❌ MISSING" if not csharp_files else "⚠️ PARTIAL"
            
            report.append(f"### {category_name} - {status}")
            report.append(f"- **Swift Tests**: {len(swift_files)} files, {swift_count} methods")
            report.append(f"- **C# Tests**: {len(csharp_files)} files, {csharp_count} methods")
            
            if csharp_count > 0 and swift_count > 0:
                enhancement = round(csharp_count / swift_count, 2)
                report.append(f"- **Enhancement Factor**: {enhancement}x")
            
            # List files
            if swift_files:
                report.append("- **Swift Files**:")
                for file_path, methods in swift_files.items():
                    report.append(f"  - `{Path(file_path).name}` ({len(methods)} tests)")
            
            if csharp_files:
                report.append("- **C# Files**:")
                for file_path, methods in csharp_files.items():
                    report.append(f"  - `{Path(file_path).name}` ({len(methods)} tests)")
            
            report.append("")
        
        # Missing coverage details
        report.append("## 🚨 MISSING TEST COVERAGE ANALYSIS")
        report.append("")
        
        if missing_coverage['missing_categories']:
            report.append("### ❌ COMPLETELY MISSING CATEGORIES")
            for missing_cat in missing_coverage['missing_categories']:
                report.append(f"- **{self.categories.get(missing_cat['category'], missing_cat['category'])}**: {missing_cat['test_count']} tests missing")
                for file_name in missing_cat['files']:
                    report.append(f"  - `{Path(file_name).name}`")
            report.append("")
        
        if missing_coverage['missing_files']:
            report.append("### ⚠️ MISSING TEST FILES")
            for missing_file in missing_coverage['missing_files']:
                report.append(f"- **{self.categories.get(missing_file['category'], missing_file['category'])}**: `{Path(missing_file['file']).name}` ({missing_file['missing_methods']} tests)")
            report.append("")
        
        # C# Enhancements
        if missing_coverage['c_sharp_enhancements']:
            report.append("### 🚀 C# TEST ENHANCEMENTS (BEYOND SWIFT PARITY)")
            for enhancement in missing_coverage['c_sharp_enhancements']:
                report.append(f"- **{self.categories.get(enhancement['category'], enhancement['category'])}**: {enhancement['enhancement_factor']}x enhancement ({enhancement['csharp_tests']} vs {enhancement['swift_tests']} tests)")
            report.append("")
        
        # Recommendations
        report.append("## 📋 IMPLEMENTATION RECOMMENDATIONS")
        report.append("")
        
        total_missing = len(missing_coverage['missing_categories']) + len(missing_coverage['missing_files'])
        if total_missing > 0:
            report.append(f"### 🎯 PRIORITY ACTIONS ({total_missing} gaps identified)")
            report.append("")
            
            # High priority missing categories
            if missing_coverage['missing_categories']:
                report.append("**HIGH PRIORITY - Missing Categories:**")
                for missing_cat in missing_coverage['missing_categories']:
                    report.append(f"1. Implement {self.categories.get(missing_cat['category'], missing_cat['category'])} ({missing_cat['test_count']} tests)")
                report.append("")
            
            # Medium priority missing files  
            if missing_coverage['missing_files']:
                report.append("**MEDIUM PRIORITY - Missing Files:**")
                for missing_file in missing_coverage['missing_files']:
                    report.append(f"1. Create `{Path(missing_file['file']).name.replace('.swift', '.cs')}` ({missing_file['missing_methods']} tests)")
                report.append("")
        
        # Final verdict
        coverage_percentage = round((csharp_total_methods / swift_total_methods) * 100 if swift_total_methods > 0 else 0, 1)
        report.append("## 🏁 FINAL VERDICT")
        report.append("")
        
        if coverage_percentage >= 100 and total_missing == 0:
            report.append("### ✅ TEST COVERAGE: COMPLETE ✅")
            report.append("C# implementation has **100% test parity** with Swift plus enhancements!")
        elif coverage_percentage >= 80:
            report.append("### ⚠️ TEST COVERAGE: SUBSTANTIAL ⚠️")  
            report.append(f"C# implementation has **{coverage_percentage}% coverage** of Swift tests with {total_missing} gaps")
        else:
            report.append("### ❌ TEST COVERAGE: INSUFFICIENT ❌")
            report.append(f"C# implementation has only **{coverage_percentage}% coverage** of Swift tests")
        
        report.append(f"**Overall Enhancement Factor**: {round(csharp_total_methods/swift_total_methods if swift_total_methods > 0 else 0, 2)}x")
        report.append("")
        
        return "\n".join(report)

def main():
    analyzer = TestCoverageAnalyzer()
    
    # Analyze Swift tests
    swift_test_dir = "/home/neo/git/NeoUnity/NeoSwift/Tests/NeoSwiftTests/unit"
    analyzer.analyze_swift_tests(swift_test_dir)
    
    # Analyze C# tests  
    csharp_test_dir = "/home/neo/git/NeoUnity/Tests/Runtime"
    analyzer.analyze_csharp_tests(csharp_test_dir)
    
    # Generate comprehensive report
    report = analyzer.generate_comprehensive_report()
    
    # Save and display report
    with open("/home/neo/git/NeoUnity/TEST_COVERAGE_MATRIX.md", "w") as f:
        f.write(report)
    
    print(report)

if __name__ == "__main__":
    main()