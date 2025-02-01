﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WashDelivery.Infrastructure.Data;

#nullable disable

namespace WashDelivery.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250104190752_init")]
    partial class init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Address", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AdditionalInstructions")
                        .HasColumnType("TEXT");

                    b.Property<string>("ApartmentNumber")
                        .HasColumnType("TEXT");

                    b.Property<string>("BuildingNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Latitude")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Longitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Admin", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.CustomerDeliveryAddress", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AdditionalInstructions")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("ApartmentNumber")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("BuildingNumber")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Latitude")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Longitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("CustomerDeliveryAddresses");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.DraftOrder", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CourierInstructions")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("TEXT");

                    b.Property<bool>("LeaveAtDoor")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("PickupTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("DraftOrders");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Laundry", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ContactEmail")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("ContactPhone")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<double>("Rating")
                        .HasPrecision(3, 2)
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Laundries");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.LaundryService", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsExtraService")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LaundryId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("LaundryId");

                    b.ToTable("LaundryService");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CourierId")
                        .HasColumnType("TEXT");

                    b.Property<string>("CourierInstructions")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("FinalPrice")
                        .HasPrecision(10, 2)
                        .HasColumnType("TEXT");

                    b.Property<string>("LaundryId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("LeaveAtDoor")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PickupTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.OrderHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderHistory");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.OrderItem", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("DraftOrderId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsExtra")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("OrderId")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasPrecision(10, 2)
                        .HasColumnType("TEXT");

                    b.Property<string>("ServiceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Weight")
                        .HasPrecision(10, 3)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DraftOrderId");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderItem");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.OrderStatusHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ChangedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasColumnType("TEXT");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderStatusHistories");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LaundryId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("UserType")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasDiscriminator<string>("UserType").HasValue("User");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Courier", b =>
                {
                    b.HasBaseType("WashDelivery.Domain.Entities.User");

                    b.Property<bool>("IsAssigned")
                        .HasColumnType("INTEGER");

                    b.HasIndex("LaundryId");

                    b.HasDiscriminator().HasValue("Courier");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Customer", b =>
                {
                    b.HasBaseType("WashDelivery.Domain.Entities.User");

                    b.Property<decimal>("Rating")
                        .HasColumnType("TEXT");

                    b.HasIndex("LaundryId");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasDiscriminator().HasValue("Customer");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.LaundryManager", b =>
                {
                    b.HasBaseType("WashDelivery.Domain.Entities.User");

                    b.HasIndex("LaundryId");

                    b.HasDiscriminator().HasValue("LaundryManager");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.LaundryWorker", b =>
                {
                    b.HasBaseType("WashDelivery.Domain.Entities.User");

                    b.HasIndex("LaundryId");

                    b.HasDiscriminator().HasValue("LaundryWorker");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WashDelivery.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Address", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.User", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Admin", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.User", "User")
                        .WithOne()
                        .HasForeignKey("WashDelivery.Domain.Entities.Admin", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.CustomerDeliveryAddress", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Customer", null)
                        .WithMany("Addresses")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.DraftOrder", b =>
                {
                    b.OwnsOne("WashDelivery.Domain.ValueObjects.OrderAddress", "DeliveryAddress", b1 =>
                        {
                            b1.Property<string>("DraftOrderId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("AdditionalInstructions")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ApartmentNumber")
                                .HasColumnType("TEXT");

                            b1.Property<string>("BuildingNumber")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Latitude")
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Longitude")
                                .HasColumnType("TEXT");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("DraftOrderId");

                            b1.ToTable("DraftOrders");

                            b1.WithOwner()
                                .HasForeignKey("DraftOrderId");
                        });

                    b.OwnsOne("WashDelivery.Domain.ValueObjects.OrderAddress", "PickupAddress", b1 =>
                        {
                            b1.Property<string>("DraftOrderId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("AdditionalInstructions")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ApartmentNumber")
                                .HasColumnType("TEXT");

                            b1.Property<string>("BuildingNumber")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Latitude")
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Longitude")
                                .HasColumnType("TEXT");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("DraftOrderId");

                            b1.ToTable("DraftOrders");

                            b1.WithOwner()
                                .HasForeignKey("DraftOrderId");
                        });

                    b.Navigation("DeliveryAddress");

                    b.Navigation("PickupAddress");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Laundry", b =>
                {
                    b.OwnsOne("WashDelivery.Domain.ValueObjects.LocationAddress", "Address", b1 =>
                        {
                            b1.Property<string>("LaundryId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<double>("Latitude")
                                .HasColumnType("REAL");

                            b1.Property<double>("Longitude")
                                .HasColumnType("REAL");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("LaundryId");

                            b1.ToTable("Laundries");

                            b1.WithOwner()
                                .HasForeignKey("LaundryId");
                        });

                    b.Navigation("Address")
                        .IsRequired();
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.LaundryService", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Laundry", null)
                        .WithMany("Services")
                        .HasForeignKey("LaundryId");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Order", b =>
                {
                    b.OwnsOne("WashDelivery.Domain.ValueObjects.OrderAddress", "DeliveryAddress", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("AdditionalInstructions")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ApartmentNumber")
                                .HasColumnType("TEXT");

                            b1.Property<string>("BuildingNumber")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Latitude")
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Longitude")
                                .HasColumnType("TEXT");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("OrderId");

                            b1.ToTable("Orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.OwnsOne("WashDelivery.Domain.ValueObjects.OrderAddress", "PickupAddress", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("AdditionalInstructions")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ApartmentNumber")
                                .HasColumnType("TEXT");

                            b1.Property<string>("BuildingNumber")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Latitude")
                                .HasColumnType("TEXT");

                            b1.Property<decimal>("Longitude")
                                .HasColumnType("TEXT");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("OrderId");

                            b1.ToTable("Orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.Navigation("DeliveryAddress")
                        .IsRequired();

                    b.Navigation("PickupAddress")
                        .IsRequired();
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.OrderHistory", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.OrderItem", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.DraftOrder", "DraftOrder")
                        .WithMany("Items")
                        .HasForeignKey("DraftOrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WashDelivery.Domain.Entities.Order", "Order")
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("DraftOrder");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.OrderStatusHistory", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Order", "Order")
                        .WithMany("StatusHistory")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Courier", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Laundry", "Laundry")
                        .WithMany()
                        .HasForeignKey("LaundryId")
                        .HasConstraintName("FK_AspNetUsers_Laundries_LaundryId3");

                    b.Navigation("Laundry");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Customer", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Laundry", "Laundry")
                        .WithMany()
                        .HasForeignKey("LaundryId")
                        .HasConstraintName("FK_AspNetUsers_Laundries_LaundryId2");

                    b.Navigation("Laundry");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.LaundryManager", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Laundry", "Laundry")
                        .WithMany()
                        .HasForeignKey("LaundryId")
                        .HasConstraintName("FK_AspNetUsers_Laundries_LaundryId1");

                    b.Navigation("Laundry");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.LaundryWorker", b =>
                {
                    b.HasOne("WashDelivery.Domain.Entities.Laundry", "Laundry")
                        .WithMany("Workers")
                        .HasForeignKey("LaundryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Laundry");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.DraftOrder", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Laundry", b =>
                {
                    b.Navigation("Services");

                    b.Navigation("Workers");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Order", b =>
                {
                    b.Navigation("Items");

                    b.Navigation("StatusHistory");
                });

            modelBuilder.Entity("WashDelivery.Domain.Entities.Customer", b =>
                {
                    b.Navigation("Addresses");
                });
#pragma warning restore 612, 618
        }
    }
}
