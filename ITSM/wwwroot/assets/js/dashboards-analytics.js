/**
 * Dashboard Analytics
 */

'use strict';

document.addEventListener('DOMContentLoaded', function (e) {
  let cardColor, headingColor, legendColor, labelColor, shadeColor, borderColor, fontFamily;
  cardColor = config.colors.cardColor;
  headingColor = config.colors.headingColor;
  legendColor = config.colors.white;
  labelColor = config.colors.textMuted;
  borderColor = config.colors.borderColor;
  fontFamily = config.fontFamily;

  // Order Area Chart
  // --------------------------------------------------------------------
  const orderAreaChartEl = document.querySelector('#orderChart'),
    orderAreaChartConfig = {
      chart: {
        height: 80,
        type: 'area',
        toolbar: {
          show: false
        },
        sparkline: {
          enabled: true
        }
      },
      markers: {
        size: 6,
        colors: 'transparent',
        strokeColors: 'transparent',
        strokeWidth: 4,
        discrete: [
          {
            fillColor: cardColor,
            seriesIndex: 0,
            dataPointIndex: 6,
            strokeColor: config.colors.success,
            strokeWidth: 2,
            size: 6,
            radius: 8
          }
        ],
        offsetX: -1,
        hover: {
          size: 7
        }
      },
      grid: {
        show: false,
        padding: {
          top: 15,
          right: 7,
          left: 0
        }
      },
      colors: [config.colors.success],
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.4,
          gradientToColors: [config.colors.cardColor],
          opacityTo: 0.4,
          stops: [0, 100]
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 2,
        curve: 'smooth'
      },
      series: [
        {
          data: [180, 175, 275, 140, 205, 190, 295]
        }
      ],
      xaxis: {
        show: false,
        lines: {
          show: false
        },
        labels: {
          show: false
        },
        stroke: {
          width: 0
        },
        axisBorder: {
          show: false
        }
      },
      yaxis: {
        stroke: {
          width: 0
        },
        show: false
      }
    };
  if (typeof orderAreaChartEl !== undefined && orderAreaChartEl !== null) {
    const orderAreaChart = new ApexCharts(orderAreaChartEl, orderAreaChartConfig);
    orderAreaChart.render();
  }

  // Total Revenue Report Chart - Bar Chart
  // --------------------------------------------------------------------
  const totalRevenueChartEl = document.querySelector('#totalRevenueChart'),
    totalRevenueChartOptions = {
      series: [
        {
          name: "Other State",
          data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]  // 默认12个月都是0
        },
        {
          name: "Resolved",
          data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]  // 默认12个月都是0
        }
      ],
      chart: {
        height: 300,
        type: 'bar',
        stacked: true,
        toolbar: { show: false }
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '30%',
          borderRadius: 8,
          startingShape: 'rounded',
          endingShape: 'rounded'
        }
      },
      colors: [config.colors.info, config.colors.primary],
      dataLabels: {
        enabled: false
      },
      stroke: {
        curve: 'smooth',
        width: 6,
        lineCap: 'round',
        colors: [cardColor]
      },
      legend: {
        show: true,
        horizontalAlign: 'left',
        position: 'top',
        markers: {
          size: 4,
          radius: 12,
          shape: 'circle',
          strokeWidth: 0
        },
        fontSize: '13px',
        fontFamily: fontFamily,
        fontWeight: 400,
        labels: {
          colors: legendColor,
          useSeriesColors: false
        },
        itemMargin: {
          horizontal: 10
        }
      },
      grid: {
        strokeDashArray: 7,
        borderColor: borderColor,
        padding: {
          top: 0,
          bottom: -8,
          left: 20,
          right: 20
        }
      },
      xaxis: {
        categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        labels: {
          style: {
            fontSize: '13px',
            fontFamily: fontFamily,
            colors: labelColor
          }
        },
        axisTicks: {
          show: false
        },
        axisBorder: {
          show: false
        }
      },
      yaxis: {
        labels: {
          style: {
            fontSize: '13px',
            fontFamily: fontFamily,
            colors: labelColor
          }
        }
      },
      states: {
        hover: {
          filter: {
            type: 'none'
          }
        },
        active: {
          filter: {
            type: 'none'
          }
        }
      }
    };
  if (typeof totalRevenueChartEl !== undefined && totalRevenueChartEl !== null) {
    const totalRevenueChart = new ApexCharts(totalRevenueChartEl, totalRevenueChartOptions);
    totalRevenueChart.render();

    // 获取当前年份
    const currentYear = new Date().getFullYear();

    // 更新图表数据的函数
    function updateChartData(year) {
      const statsElement = document.querySelector('#incidentStatsData');
      if (statsElement && statsElement.dataset.stats) {
        try {
          const statsData = JSON.parse(statsElement.dataset.stats);
          const yearData = statsData.find(stat => stat.year === year);

          if (yearData) {
            const resolvedData = [];
            const otherData = [];

            // 获取1-12月的数据
            for (let month = 1; month <= 12; month++) {
              if (yearData.monthlyData && yearData.monthlyData[month]) {
                resolvedData.push(yearData.monthlyData[month].resolvedCount);
                otherData.push(-yearData.monthlyData[month].otherCount); // 使Other State为负值
              } else {
                resolvedData.push(0);
                otherData.push(0);
              }
            }

            // 更新图表数据
            totalRevenueChart.updateOptions({
              yaxis: {
                labels: {
                  formatter: function(val) {
                    return Math.abs(val); // 显示绝对值
                  }
                }
              }
            });

            totalRevenueChart.updateSeries([
              {
                name: "Other State",
                data: otherData
              },
              {
                name: "Resolved",
                data: resolvedData
              }
            ]);

            // 更新Growth显示
            const growthText = document.querySelector('.text-center.fw-medium.my-6');
            if (growthText) {
              growthText.textContent = `${yearData.growth}% Resolved Rate`;
            }

            // 更新Growth图表
            if (growthChart) {
              growthChart.updateSeries([yearData.growth]);
            }

            // 更新底部年份显示
            const allYears = statsData.map(stat => stat.year).sort((a, b) => b - a); // 降序排序
            const selectedIndex = allYears.indexOf(year);
            
            // 获取要显示的两个年份（排除当前选中的年份）
            let displayYears = allYears.filter(y => y !== year);
            
            // 确保年份按降序排序
            displayYears.sort((a, b) => b - a);
            
            // 限制只取两个年份
            displayYears = displayYears.slice(0, 2);
            
            // 更新年份显示和Growth值
            const [firstYear, secondYear] = displayYears;
            
            // 更新第一个显示框（较大的年份）
            const firstYearElement = document.querySelector('.prev-year');
            const firstGrowthValue = document.querySelectorAll('.growth-value')[0];
            if (firstYearElement && firstYear) {
                firstYearElement.textContent = firstYear;
                const firstYearData = statsData.find(stat => stat.year === firstYear);
                if (firstYearData && firstGrowthValue) {
                    firstGrowthValue.textContent = firstYearData.growth.toFixed(1) + '%';
                }
            }

            // 更新第二个显示框（较小的年份）
            const secondYearElement = document.querySelector('.next-year');
            const secondGrowthValue = document.querySelectorAll('.growth-value')[1];
            if (secondYearElement && secondYear) {
                secondYearElement.textContent = secondYear;
                const secondYearData = statsData.find(stat => stat.year === secondYear);
                if (secondYearData && secondGrowthValue) {
                    secondGrowthValue.textContent = secondYearData.growth.toFixed(1) + '%';
                }
            }
          }
        } catch (error) {
          console.error('Error parsing or processing stats data:', error);
        }
      }
    }

    // 添加年份选择事件监听
    document.querySelectorAll('.year-item').forEach(item => {
      item.addEventListener('click', function() {
        const selectedYear = parseInt(this.dataset.year);
        // 更新图表数据
        updateChartData(selectedYear);
        
        // 更新所有显示年份的元素
        const yearDisplayElements = document.querySelectorAll('#selectedYear');
        yearDisplayElements.forEach(element => {
          element.textContent = selectedYear;
        });

        // 更新右侧按钮的年份显示
        const yearButtons = document.querySelectorAll('.btn-group .btn.btn-outline-primary:not(.dropdown-toggle)');
        yearButtons.forEach(button => {
          button.textContent = selectedYear;
        });

        // 更新下拉按钮的选中状态
        document.querySelectorAll('.year-item').forEach(yearItem => {
          if (parseInt(yearItem.dataset.year) === selectedYear) {
            yearItem.classList.add('active');
          } else {
            yearItem.classList.remove('active');
          }
        });
      });
    });

    // 初始化时设置当前年份为选中状态
    const currentYearItems = document.querySelectorAll(`.year-item[data-year="${currentYear}"]`);
    currentYearItems.forEach(item => item.classList.add('active'));
  }

  // Growth Chart - Radial Bar Chart
  // --------------------------------------------------------------------
  const growthChartEl = document.querySelector('#growthChart'),
    growthChartOptions = {
      series: [parseFloat(document.querySelector('.text-center.fw-medium.my-6').textContent)], // 获取当前年份的Growth值
      labels: ['Growth'],
      chart: {
        height: 200,
        type: 'radialBar'
      },
      plotOptions: {
        radialBar: {
          size: 150,
          offsetY: 10,
          startAngle: -150,
          endAngle: 150,
          hollow: {
            size: '55%'
          },
          track: {
            background: cardColor,
            strokeWidth: '100%'
          },
          dataLabels: {
            name: {
              offsetY: 15,
              color: legendColor,
              fontSize: '15px',
              fontWeight: '500',
              fontFamily: fontFamily
            },
            value: {
              offsetY: -25,
              color: legendColor,
              fontSize: '22px',
              fontWeight: '500',
              fontFamily: fontFamily
            }
          }
        }
      },
      colors: [config.colors.primary],
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'dark',
          shadeIntensity: 0.5,
          gradientToColors: [config.colors.primary],
          inverseColors: true,
          opacityFrom: 1,
          opacityTo: 0.6,
          stops: [30, 70, 100]
        }
      },
      stroke: {
        dashArray: 5
      },
      grid: {
        padding: {
          top: -35,
          bottom: -10
        }
      },
      states: {
        hover: {
          filter: {
            type: 'none'
          }
        },
        active: {
          filter: {
            type: 'none'
          }
        }
      }
    };

  let growthChart;
  if (typeof growthChartEl !== undefined && growthChartEl !== null) {
    growthChart = new ApexCharts(growthChartEl, growthChartOptions);
    growthChart.render();
  }

  // Revenue Bar Chart
  // --------------------------------------------------------------------
  const revenueBarChartEl = document.querySelector('#revenueChart'),
    revenueBarChartConfig = {
      chart: {
        height: 95,
        type: 'bar',
        toolbar: {
          show: false
        }
      },
      plotOptions: {
        bar: {
          barHeight: '80%',
          columnWidth: '75%',
          startingShape: 'rounded',
          endingShape: 'rounded',
          borderRadius: 4,
          distributed: true
        }
      },
      grid: {
        show: false,
        padding: {
          top: -20,
          bottom: -12,
          left: -10,
          right: 0
        }
      },
      colors: [
        config.colors.primary,
        config.colors.primary,
        config.colors.primary,
        config.colors.primary,
        config.colors.primary,
        config.colors.primary,
        config.colors.primary
      ],
      dataLabels: {
        enabled: false
      },
      series: [
        {
          data: [40, 95, 60, 45, 90, 50, 75]
        }
      ],
      legend: {
        show: false
      },
      xaxis: {
        categories: ['M', 'T', 'W', 'T', 'F', 'S', 'S'],
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        },
        labels: {
          style: {
            colors: labelColor,
            fontSize: '13px'
          }
        }
      },
      yaxis: {
        labels: {
          show: false
        }
      }
    };
  if (typeof revenueBarChartEl !== undefined && revenueBarChartEl !== null) {
    const revenueBarChart = new ApexCharts(revenueBarChartEl, revenueBarChartConfig);
    revenueBarChart.render();
  }

  // Profit Report Line Chart
  // --------------------------------------------------------------------
  const profileReportChartEl = document.querySelector('#profileReportChart'),
    profileReportChartConfig = {
      chart: {
        height: 75,
        width: 240,
        type: 'line',
        toolbar: {
          show: false
        },
        dropShadow: {
          enabled: true,
          top: 10,
          left: 5,
          blur: 3,
          color: config.colors.warning,
          opacity: 0.15
        },
        sparkline: {
          enabled: true
        }
      },
      grid: {
        show: false,
        padding: {
          right: 8
        }
      },
      colors: [config.colors.warning],
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 5,
        curve: 'smooth'
      },
      series: [
        {
          data: [110, 270, 145, 245, 205, 285]
        }
      ],
      xaxis: {
        show: false,
        lines: {
          show: false
        },
        labels: {
          show: false
        },
        axisBorder: {
          show: false
        }
      },
      yaxis: {
        show: false
      },
      responsive: [
        {
          breakpoint: 1700,
          options: {
            chart: {
              width: 200
            }
          }
        },
        {
          breakpoint: 1579,
          options: {
            chart: {
              width: 180
            }
          }
        },
        {
          breakpoint: 1500,
          options: {
            chart: {
              width: 160
            }
          }
        },
        {
          breakpoint: 1450,
          options: {
            chart: {
              width: 140
            }
          }
        },
        {
          breakpoint: 1400,
          options: {
            chart: {
              width: 240
            }
          }
        }
      ]
    };
  if (typeof profileReportChartEl !== undefined && profileReportChartEl !== null) {
    const profileReportChart = new ApexCharts(profileReportChartEl, profileReportChartConfig);
    profileReportChart.render();
  }

  // Order Statistics Chart
  // --------------------------------------------------------------------
    const chartData = document.getElementById("chart-data");

    const incident = parseInt(chartData.dataset.incident);
    const request = parseInt(chartData.dataset.request);
    const feedback = parseInt(chartData.dataset.feedback);
    const knowledge = parseInt(chartData.dataset.knowledge);
    const total = parseInt(chartData.dataset.total);

    const chartOrderStatistics = document.querySelector('#orderStatisticsChart'),
        orderChartConfig = {
            chart: {
                height: 165,
                width: 136,
                type: 'donut',
                offsetX: 15
            },
            labels: ['Incidents', 'Requests', 'Feedbacks', 'Knowledges'],
            series: [incident, request, feedback, knowledge],
            colors: [config.colors.success, config.colors.primary, config.colors.secondary, config.colors.info],
            stroke: {
                width: 5,
                colors: [cardColor]
            },
            dataLabels: {
                enabled: false,
                formatter: function (val, opt) {
                    return parseInt(val) + '%';
                }
            },
            legend: {
                show: false
            },
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    right: 15
                }
            },
            states: {
                hover: {
                    filter: { type: 'none' }
                },
                active: {
                    filter: { type: 'none' }
                }
            },
            plotOptions: {
                pie: {
                    donut: {
                        size: '75%',
                        labels: {
                            show: true,
                            value: {
                                fontSize: '1.125rem',
                                fontFamily: fontFamily,
                                fontWeight: 500,
                                color: legendColor,
                                offsetY: -17,
                                formatter: function (val) {
                                    return parseInt(val) + '%';
                                }
                            },
                            name: {
                                offsetY: 17,
                                fontFamily: fontFamily
                            },
                            total: {
                                show: true,
                                fontSize: '13px',
                                color: legendColor,
                                label: 'Weekly',
                                formatter: function (w) {
                                    return `${total}%`;
                                }
                            }
                        }
                    }
                }
            }
        };
    if (typeof chartOrderStatistics !== undefined && chartOrderStatistics !== null) {
        const statisticsChart = new ApexCharts(chartOrderStatistics, orderChartConfig);
        statisticsChart.render();
    }

  // Income Chart - Area chart
  // --------------------------------------------------------------------
  const incomeChartEl = document.querySelector('#incomeChart'),
    incomeChartConfig = {
      series: [
        {
          data: [21, 30, 22, 42, 26, 35, 29,30,21,15,28,45]
        }
      ],
      chart: {
        height: 200,
        parentHeightOffset: 0,
        parentWidthOffset: 0,
        toolbar: {
          show: false
        },
        type: 'area'
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: 3,
        curve: 'smooth'
      },
      legend: {
        show: false
      },
      markers: {
        size: 6,
        colors: 'transparent',
        strokeColors: 'transparent',
        strokeWidth: 4,
        discrete: [
          {
            fillColor: config.colors.white,
            seriesIndex: 0,
            dataPointIndex: 6,
            strokeColor: config.colors.primary,
            strokeWidth: 2,
            size: 6,
            radius: 8
          }
        ],
        offsetX: -1,
        hover: {
          size: 7
        }
      },
      colors: [config.colors.primary],
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.3,
          gradientToColors: [config.colors.cardColor],
          opacityTo: 0.3,
          stops: [0, 100]
        }
      },
      grid: {
        borderColor: borderColor,
        strokeDashArray: 8,
        padding: {
          top: -20,
          bottom: -8,
          left: 0,
          right: 8
        }
      },
      xaxis: {
          categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        axisBorder: {
          show: false
        },
        axisTicks: {
          show: false
        },
        labels: {
          show: true,
          style: {
            fontSize: '13px',
            colors: labelColor
          }
        }
      },
      yaxis: {
        labels: {
          show: false
        },
        min: 10,
        max: 50,
        tickAmount: 4
      }
    };
  if (typeof incomeChartEl !== undefined && incomeChartEl !== null) {
    const incomeChart = new ApexCharts(incomeChartEl, incomeChartConfig);
    incomeChart.render();
  }

  // Expenses Mini Chart - Radial Chart
  // --------------------------------------------------------------------
  const weeklyExpensesEl = document.querySelector('#expensesOfWeek'),
    weeklyExpensesConfig = {
      series: [65],
      chart: {
        width: 60,
        height: 60,
        type: 'radialBar'
      },
      plotOptions: {
        radialBar: {
          startAngle: 0,
          endAngle: 360,
          strokeWidth: '8',
          hollow: {
            margin: 2,
            size: '40%'
          },
          track: {
            background: borderColor
          },
          dataLabels: {
            show: true,
            name: {
              show: false
            },
            value: {
              formatter: function (val) {
                return '$' + parseInt(val);
              },
              offsetY: 5,
              color: legendColor,
              fontSize: '12px',
              fontFamily: fontFamily,
              show: true
            }
          }
        }
      },
      fill: {
        type: 'solid',
        colors: config.colors.primary
      },
      stroke: {
        lineCap: 'round'
      },
      grid: {
        padding: {
          top: -10,
          bottom: -15,
          left: -10,
          right: -10
        }
      },
      states: {
        hover: {
          filter: {
            type: 'none'
          }
        },
        active: {
          filter: {
            type: 'none'
          }
        }
      }
    };
  if (typeof weeklyExpensesEl !== undefined && weeklyExpensesEl !== null) {
    const weeklyExpenses = new ApexCharts(weeklyExpensesEl, weeklyExpensesConfig);
    weeklyExpenses.render();
  }
});
